using Aihrly.Api.Data;
using Aihrly.Api.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aihrly.Api.Tests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"AihrlyTest_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AihrlyDbContext>>();
            services.RemoveAll<AihrlyDbContext>();
            services.RemoveAll<IDbContextOptionsConfiguration<AihrlyDbContext>>();

            services.AddDbContext<AihrlyDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<IDistributedCache>();
            services.AddDistributedMemoryCache();

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AihrlyDbContext>();
            db.Database.EnsureCreated();

            if (!db.TeamMembers.Any())
            {
                db.TeamMembers.AddRange(
                    new TeamMember
                    {
                        Id = 1,
                        Name = "Alex Johnson",
                        Email = "alex.johnson@aihrly.test",
                        Role = TeamMemberRole.Recruiter
                    },
                    new TeamMember
                    {
                        Id = 2,
                        Name = "Sam Patel",
                        Email = "sam.patel@aihrly.test",
                        Role = TeamMemberRole.HiringManager
                    },
                    new TeamMember
                    {
                        Id = 3,
                        Name = "Jordan Lee",
                        Email = "jordan.lee@aihrly.test",
                        Role = TeamMemberRole.Recruiter
                    });

                db.SaveChanges();
            }
        });
    }
}
