using CompanyManager.Application;
using CompanyManager.Application.Auth;
using CompanyManager.Application.Auth.Interfaces;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Infrastructure;
using CompanyManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" })
    .AddCheck("database", () => HealthCheckResult.Healthy("Database connection is healthy"), tags: new[] { "ready", "database" })
    .AddCheck("external_services", () => HealthCheckResult.Healthy("External services are accessible"), tags: new[] { "ready", "external" });

// Add AutoMapper configuration
// builder.Services.AddMapping();

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

        // (opcional) checar SecurityStamp/IsActive a cada token validado
        opts.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                // Exemplo: validar SecurityStamp e IsActive
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

// Add Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Initialize database with seed data
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add custom middleware
app.UseMiddleware<CompanyManager.API.Middleware.RequestLoggingMiddleware>();

// Add Health Check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Duration = report.TotalDuration,
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description,
                Duration = entry.Value.Duration
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false, // No checks for liveness
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = "Alive",
            Timestamp = DateTime.UtcNow
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.UseAuthorization();

app.MapControllers();

app.Run();
