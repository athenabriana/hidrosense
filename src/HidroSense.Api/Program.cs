using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using HidroSense.Api.Data;
using HidroSense.Api.Middleware;
using HidroSense.Api.Repositories;
using HidroSense.Api.Security;
using HidroSense.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Secrets come from environment variables (Jwt__SecretKey, Seed__AdminPassword...), never from appsettings.
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(s => s.SecretKey.Length >= 32, "Jwt:SecretKey ausente ou com menos de 32 caracteres. Defina a variável Jwt__SecretKey.")
    .ValidateOnStart();

builder.Services.AddOptions<SeedSettings>()
    .Bind(builder.Configuration.GetSection("Seed"))
    .Validate(s => !string.IsNullOrWhiteSpace(s.AdminPassword) && !string.IsNullOrWhiteSpace(s.OperatorPassword),
        "Seed:AdminPassword e Seed:OperatorPassword são obrigatórios. Defina Seed__AdminPassword e Seed__OperatorPassword.")
    .ValidateOnStart();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IConsumerUnitRepository, ConsumerUnitRepository>();
builder.Services.AddScoped<IConsumptionRepository, ConsumptionRepository>();
builder.Services.AddScoped<ILeakAlertRepository, LeakAlertRepository>();
builder.Services.AddScoped<IConsumerUnitService, ConsumerUnitService>();
builder.Services.AddScoped<IConsumptionService, ConsumptionService>();
builder.Services.AddScoped<ILeakAlertService, LeakAlertService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Resolved lazily so configuration injected by hosts and test factories is honored.
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((options, jwt) =>
    {
        var settings = jwt.Value;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HidroSense API", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// On Kubernetes the API may start before SQL Server accepts connections, and replicas
// race on the first migration/seed; retrying lets the loser converge instead of crash-looping.
const int maxStartupAttempts = 12;
for (var attempt = 1; ; attempt++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Database.IsRelational())
            await db.Database.MigrateAsync();
        var seed = scope.ServiceProvider.GetRequiredService<IOptions<SeedSettings>>().Value;
        await DbSeeder.SeedAsync(db, seed);
        break;
    }
    catch (Exception ex) when (attempt < maxStartupAttempts && ex is not OptionsValidationException)
    {
        app.Logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{Max}), retrying in 5s", attempt, maxStartupAttempts);
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}

app.Run();

public partial class Program { }
