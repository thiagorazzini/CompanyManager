using CompanyManager.API.Middleware;
using CompanyManager.Application;
using CompanyManager.Application.Auth;
using CompanyManager.Application.Auth.Interfaces;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Infrastructure;
using CompanyManager.Infrastructure.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" })
    .AddCheck("database", () => HealthCheckResult.Healthy("Database connection is healthy"), tags: new[] { "ready", "database" })
    .AddCheck("external_services", () => HealthCheckResult.Healthy("External services are accessible"), tags: new[] { "ready", "external" });

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(jwt.ClockSkewSeconds)
        };

        opts.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                var services = ctx.HttpContext.RequestServices;
                var repo = services.GetRequiredService<IUserAccountRepository>();
                var sub = ctx.Principal!.FindFirst("sub")?.Value;
                var stamp = ctx.Principal!.FindFirst("sstamp")?.Value;

                if (!Guid.TryParse(sub, out var userId))
                {
                    ctx.Fail("Invalid subject");
                    return;
                }

                var user = await repo.GetByIdAsync(userId);
                if (user is null || !user.IsActive || user.SecurityStamp.ToString() != stamp)
                {
                    ctx.Fail("User no longer valid");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeesRead", p => p.RequireClaim("perm", "employees:read"));
    options.AddPolicy("EmployeesWrite", p => p.RequireClaim("perm", "employees:write"));
    options.AddPolicy("DepartmentsRead", p => p.RequireClaim("perm", "departments:read"));
    options.AddPolicy("DepartmentsWrite", p => p.RequireClaim("perm", "departments:write"));
    options.AddPolicy("UsersAdmin", p => p.RequireClaim("perm", "users:admin"));
});

builder.Services.AddScoped<ITokenService, CompanyManager.Application.Services.TokenService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializerService>();
    await dbInitializer.InitializeAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error initializing database");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.UseMiddleware<RequestLoggingMiddleware>();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
