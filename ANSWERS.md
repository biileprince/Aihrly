# Part 3 — Written Answers

## 1. Schema Design

### Tables (simplified SQL)

```sql
-- Scores are stored as nullable column groups directly on the application row.
-- This keeps the profile query to one row + two joined collections,
-- at the cost of not retaining score history (see 2b below).

CREATE TABLE applications (
    id                       SERIAL PRIMARY KEY,
    job_id                   INT NOT NULL REFERENCES jobs(id) ON DELETE CASCADE,
    candidate_name           VARCHAR(200) NOT NULL,
    candidate_email          VARCHAR(320) NOT NULL,
    cover_letter             TEXT,
    current_stage            INT NOT NULL,

    culture_fit_score        INT,
    culture_fit_comment      TEXT,
    culture_fit_updated_by   INT REFERENCES team_members(id) ON DELETE RESTRICT,
    culture_fit_updated_at   TIMESTAMPTZ,

    interview_score          INT,
    interview_comment        TEXT,
    interview_updated_by     INT REFERENCES team_members(id) ON DELETE RESTRICT,
    interview_updated_at     TIMESTAMPTZ,

    assessment_score         INT,
    assessment_comment       TEXT,
    assessment_updated_by    INT REFERENCES team_members(id) ON DELETE RESTRICT,
    assessment_updated_at    TIMESTAMPTZ,

    CONSTRAINT uq_job_email UNIQUE (job_id, candidate_email)
);

CREATE TABLE application_notes (
    id             SERIAL PRIMARY KEY,
    application_id INT NOT NULL REFERENCES applications(id) ON DELETE CASCADE,
    type           INT NOT NULL,
    description    VARCHAR(4000) NOT NULL,
    created_by_id  INT NOT NULL REFERENCES team_members(id) ON DELETE RESTRICT,
    created_at     TIMESTAMPTZ NOT NULL
);

CREATE TABLE stage_history_entries (
    id             SERIAL PRIMARY KEY,
    application_id INT NOT NULL REFERENCES applications(id) ON DELETE CASCADE,
    from_stage     INT NOT NULL,
    to_stage       INT NOT NULL,
    changed_by_id  INT NOT NULL REFERENCES team_members(id) ON DELETE RESTRICT,
    changed_at     TIMESTAMPTZ NOT NULL,
    reason         VARCHAR(1000)
);
```

### Indexes

```sql
-- Supports GET /api/jobs/{jobId}/applications (list by job, optionally filter by stage)
CREATE INDEX ix_applications_job_id ON applications(job_id);
-- Also enforces the duplicate-application rule
CREATE UNIQUE INDEX uq_applications_job_email ON applications(job_id, candidate_email);

-- Supports GET /api/applications/{id}/notes (all notes for one application)
CREATE INDEX ix_notes_application_id ON application_notes(application_id);

-- Supports stage history lookup for the profile endpoint
CREATE INDEX ix_stage_history_application_id ON stage_history_entries(application_id);
```

No index on `notes.created_by_id` or `stage_history.changed_by_id` because those columns are
only ever used as inner-join targets when resolving author names — the join drives from
`application_id`, not from the team member side.

### The profile query (`GET /api/applications/{id}`)

EF Core loads the application profile using eager-loading (`.Include().ThenInclude()`).
With the default single-query mode this compiles to one SQL statement with LEFT JOINs:

```sql
SELECT a.*,
       n.id, n.type, n.description, n.created_at,
       tm_n.name   AS note_author_name,
       sh.id, sh.from_stage, sh.to_stage, sh.changed_at, sh.reason,
       tm_sh.name  AS changed_by_name,
       tm_cf.name  AS culture_fit_updater_name,
       tm_iv.name  AS interview_updater_name,
       tm_as.name  AS assessment_updater_name
FROM   applications a
LEFT JOIN application_notes n      ON n.application_id = a.id
LEFT JOIN team_members tm_n        ON tm_n.id = n.created_by_id
LEFT JOIN stage_history_entries sh ON sh.application_id = a.id
LEFT JOIN team_members tm_sh       ON tm_sh.id = sh.changed_by_id
LEFT JOIN team_members tm_cf       ON tm_cf.id = a.culture_fit_updated_by_id
LEFT JOIN team_members tm_iv       ON tm_iv.id = a.interview_updated_by_id
LEFT JOIN team_members tm_as       ON tm_as.id = a.assessment_updated_by_id
WHERE  a.id = @id;
```

**Round trips: 1.** The score columns live on the `applications` row so no extra join is needed.
Notes and stage history each add one LEFT JOIN. All author names resolve in the same query.
Redis caching means most pipeline UI reads never hit the database at all.

---

## 2. Scoring Design Trade-offs

### (a) Three separate endpoints vs. one generic endpoint

**Why three endpoints are better here:**

Each score dimension (`culture-fit`, `interview`, `assessment`) maps to a distinct step in the
hiring process and is typically filled in by a different person at a different time — a recruiter
sets culture-fit after a phone screen, an interviewer sets the interview score after their panel,
and an assessor sets the assessment score after a take-home. Three endpoints let the client send
only the data it has, without requiring a valid value for all three dimensions simultaneously.
It also makes authorization more granular: in future you could allow only the interviewer who ran
the session to update `interview`, without touching the others.

**When one generic endpoint would be better:**

If all three scores are always set by the same person in a single workflow step (e.g. a
scorecard form submitted as one action), a single `PUT /api/applications/{id}/scores` is simpler
for the client and reduces the number of round trips. It also avoids partial-update ambiguity —
with three endpoints, sending only `culture-fit` leaves `interview` and `assessment` untouched,
which is either a feature or a footgun depending on the UI contract.

### (b) Adding score change history

**Schema change:**

Stop storing scores inline on `applications`. Replace those columns with a new `score_history`
table:

```sql
CREATE TABLE score_history (
    id             SERIAL PRIMARY KEY,
    application_id INT NOT NULL REFERENCES applications(id) ON DELETE CASCADE,
    dimension      VARCHAR(20) NOT NULL, -- 'culture_fit', 'interview', 'assessment'
    score          INT NOT NULL CHECK (score BETWEEN 1 AND 5),
    comment        TEXT,
    set_by_id      INT NOT NULL REFERENCES team_members(id) ON DELETE RESTRICT,
    set_at         TIMESTAMPTZ NOT NULL
);

CREATE INDEX ix_score_history_application_id ON score_history(application_id);
```

The "current" score for each dimension becomes `SELECT DISTINCT ON (dimension) * FROM score_history WHERE application_id = @id ORDER BY dimension, set_at DESC` — or a lateral join equivalent.

**Endpoint change:**

The three `PUT` endpoints would still be the right shape — `PUT` means "set the current value
for this dimension" and each call appends a row to `score_history` rather than overwriting.
The profile response (`GET /api/applications/{id}`) would include current scores as before,
and you'd add a new `GET /api/applications/{id}/scores/{dimension}/history` endpoint to expose
the full audit trail if the UI needs it.

---

## 3. Debugging: Candidate Appears Stuck in Screening

A recruiter reports they moved a candidate to Interview yesterday, but the system shows Screening today.

- **Reproduce exactly.** Ask the recruiter: which application ID, which browser/device, and whether the UI showed a success message at the time of the change. Knowing whether it *looked* successful narrows the search significantly.
- **Check the database first.** Query `stage_history_entries WHERE application_id = @id ORDER BY changed_at DESC`. If a row exists with `to_stage = Interview` and `from_stage = Screening`, the write landed — the problem is a display issue, not a data issue.
- **Check `applications.current_stage`.** If `stage_history_entries` has the Interview row but `applications.current_stage` is still `Screening`, the stage column was never updated — a bug in the PATCH handler where the history insert succeeded but the entity save did not, or the transaction was partial.
- **Check the Redis cache.** The profile endpoint (`GET /api/applications/{id}`) caches for 60 seconds. If the cache wasn't invalidated after the stage change, the UI would serve the old Screening value until TTL expiry. Check the cache key `application:{id}` — if it's still warm and contains `Screening`, that's the culprit.
- **Check application logs around the PATCH request timestamp.** Look for the `PATCH /api/applications/{id}/stage` log line and its response code. A `204` means the server accepted it; anything in the 4xx/5xx range means the request failed silently on the client.
- **Check for a failed transaction.** If the DB write raised an exception that was swallowed (no log, no error returned), the save never committed. Look for EF `DbUpdateException` or Npgsql timeout logs near the reported time.
- **Check the recruiter's network request.** Open browser DevTools → Network tab, filter for the PATCH call. Confirm the request was actually sent and received a 204. If the tab was closed before the response, the request may not have fired.
- **Check concurrent edits.** If a second team member moved the application back to Screening after the first move to Interview, `stage_history_entries` would show both transitions. The current stage is correct; the recruiter may have been looking at a cached or stale page.
- **Verify the X-Team-Member-Id header was valid.** If the header was missing or pointed to a non-existent team member, the filter returns 401. A client that silently ignores 4xx responses would make the UI appear to succeed while the server rejected the request.
- **Resolution path.** Once the root cause is confirmed: if it's a cache bug, force-invalidate the key and tighten the invalidation path; if it's a missing save, fix the transaction scope; if it's a client bug, fix the error handling; if the data is correct but the recruiter is confused, walk them through the stage history audit trail.

---

## 4. Honest Self-Assessment

- **C# — 2/5:** Have the basics down (async/await, DI, EF Core) but still actively learning; not yet fluent across the broader ecosystem.
- **SQL — 3/5:** Can design schemas with correct indexes and constraints; complex query tuning and window functions are areas to grow.
- **Git — 3/5:** Solid on daily workflow (branching, PRs, rebasing); less practiced with advanced history rewriting and bisect debugging.
- **REST API Design — 3/5:** Understand core principles (resource naming, status codes, problem+json); still building intuition from real project experience.
- **Writing Tests — 2/5:** Know xUnit and WebApplicationFactory basics; test design and mocking strategies are areas I am actively learning.
