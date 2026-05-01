using System.Reflection;
using Aihrly.Api.Data;
using Aihrly.Api.Filters;
using Aihrly.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AihrlyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("Redis:Configuration");
});

builder.Services.AddScoped<IApplicationProfileCache, ApplicationProfileCache>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Aihrly ATS API",
        Version = "v1",
        Description = "Team-side pipeline API for the Aihrly Applicant Tracking System. " +
                      "Mutating endpoints require an X-Team-Member-Id header containing a valid team member ID."
    });

    options.AddSecurityDefinition("TeamMemberId", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Team-Member-Id",
        Description = "ID of the acting team member (seeded IDs: 1 = Alex Johnson, 2 = Sam Patel, 3 = Jordan Lee)"
    });

    options.OperationFilter<TeamMemberSecurityOperationFilter>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AihrlyDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Aihrly ATS API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

public partial class Program { }
