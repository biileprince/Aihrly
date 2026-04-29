using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Aihrly.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    CandidateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CandidateEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    CoverLetter = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CurrentStage = table.Column<int>(type: "integer", nullable: false),
                    CultureFitScore = table.Column<int>(type: "integer", nullable: true),
                    CultureFitComment = table.Column<string>(type: "text", nullable: true),
                    CultureFitUpdatedById = table.Column<int>(type: "integer", nullable: true),
                    CultureFitUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InterviewScore = table.Column<int>(type: "integer", nullable: true),
                    InterviewComment = table.Column<string>(type: "text", nullable: true),
                    InterviewUpdatedById = table.Column<int>(type: "integer", nullable: true),
                    InterviewUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssessmentScore = table.Column<int>(type: "integer", nullable: true),
                    AssessmentComment = table.Column<string>(type: "text", nullable: true),
                    AssessmentUpdatedById = table.Column<int>(type: "integer", nullable: true),
                    AssessmentUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Applications_TeamMembers_AssessmentUpdatedById",
                        column: x => x.AssessmentUpdatedById,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Applications_TeamMembers_CultureFitUpdatedById",
                        column: x => x.CultureFitUpdatedById,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Applications_TeamMembers_InterviewUpdatedById",
                        column: x => x.InterviewUpdatedById,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationNotes_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationNotes_TeamMembers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StageHistoryEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationId = table.Column<int>(type: "integer", nullable: false),
                    FromStage = table.Column<int>(type: "integer", nullable: false),
                    ToStage = table.Column<int>(type: "integer", nullable: false),
                    ChangedById = table.Column<int>(type: "integer", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageHistoryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageHistoryEntries_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageHistoryEntries_TeamMembers_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "TeamMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TeamMembers",
                columns: new[] { "Id", "Email", "Name", "Role" },
                values: new object[,]
                {
                    { 1, "alex.johnson@aihrly.test", "Alex Johnson", 0 },
                    { 2, "sam.patel@aihrly.test", "Sam Patel", 1 },
                    { 3, "jordan.lee@aihrly.test", "Jordan Lee", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotes_ApplicationId",
                table: "ApplicationNotes",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotes_CreatedById",
                table: "ApplicationNotes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_AssessmentUpdatedById",
                table: "Applications",
                column: "AssessmentUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CultureFitUpdatedById",
                table: "Applications",
                column: "CultureFitUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_InterviewUpdatedById",
                table: "Applications",
                column: "InterviewUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobId_CandidateEmail",
                table: "Applications",
                columns: new[] { "JobId", "CandidateEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageHistoryEntries_ApplicationId",
                table: "StageHistoryEntries",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_StageHistoryEntries_ChangedById",
                table: "StageHistoryEntries",
                column: "ChangedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationNotes");

            migrationBuilder.DropTable(
                name: "StageHistoryEntries");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "TeamMembers");
        }
    }
}
