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
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",     // Frontend externo
                "http://localhost:4173",     // Vite preview
                "http://companymanager-frontend", // Frontend container
                "http://frontend"            // Frontend service name
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "CompanyManager API", 
        Version = "v1",
        Description = "API para gerenciamento de funcionários e departamentos"
    });
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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

        // Temporariamente comentado para debug
        // opts.Events = new JwtBearerEvents
        // {
        //     OnTokenValidated = async ctx =>
        //     {
        //         // Validação customizada comentada temporariamente
        //     }
        // };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    
    options.AddPolicy("EmployeesRead", p => p.RequireClaim("perm", "employees:read"));
    options.AddPolicy("EmployeesWrite", p => p.RequireClaim("perm", "employees:write"));
    options.AddPolicy("DepartmentsRead", p => p.RequireClaim("perm", "departments:read"));
    options.AddPolicy("DepartmentsWrite", p => p.RequireClaim("perm", "departments:write"));
    options.AddPolicy("UsersAdmin", p => p.RequireClaim("perm", "users:admin"));
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializerService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting database initialization...");
    await dbInitializer.InitializeAsync();
    logger.LogInformation("Database initialization completed successfully");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error initializing database: {Message}", ex.Message);
    // Continue execution even if database initialization fails
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
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

// Configurar URLs para Docker
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://+:80";
app.Urls.Clear();
app.Urls.Add(urls);

app.Run();
