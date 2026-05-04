using System.Text;
using BlueCommand.Application.Interfaces;
using BlueCommand.Application.Security;
using BlueCommand.Infrastructure.Data;
using BlueCommand.Infrastructure.Services;
using DotNetEnv;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var envPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", ".env"));
if (File.Exists(envPath))
    Env.Load(envPath);

builder.Configuration.AddEnvironmentVariables();

QuestPDF.Settings.License = LicenseType.Community;

builder.Services
    .AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Valoare invalida" : e.ErrorMessage).ToArray()
            );
        return new BadRequestObjectResult(new { errors });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlueCommand API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var connectionString = builder.Configuration["DB_CONNECTION_STRING"]
                      ?? builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Host=localhost;Port=5432;Database=bluecommand;Username=postgres;Password=postgres";

builder.Services.AddDbContext<BlueCommandDbContext>(options =>
{
    options.UseNpgsql(connectionString, o => o.EnableRetryOnFailure());
    options.UseSnakeCaseNamingConvention();
});

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? builder.Configuration["JWT_SECRET"] ?? "change-me-change-me-change-me-change-me";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BlueCommand";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "BlueCommandClient";
var jwtExpiryHours = int.TryParse(builder.Configuration["Jwt:ExpiryHours"] ?? "8", out var h) ? h : 8;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("SefSauAdmin", policy => policy.RequireRole("Administrator", "SefInspectorat"));
    options.AddPolicy("TotiUtilizatorii", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ICurrentUserService, BlueCommand.API.Services.CurrentUserService>();
builder.Services.AddScoped<IJwtTokenService>(sp =>
    new BlueCommand.API.Services.JwtTokenService(jwtSecret, jwtIssuer, jwtAudience));
builder.Services.AddSingleton(new BlueCommand.API.Services.JwtSettings(jwtExpiryHours));

var app = builder.Build();

app.UseMiddleware<BlueCommand.API.Middleware.ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/uploads"), branch =>
{
    branch.UseMiddleware<BlueCommand.API.Middleware.UploadsAuthorizationMiddleware>();
    branch.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
            Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "uploads")),
        RequestPath = "/uploads"
    });
});

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BlueCommandDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DatabaseInitializer.MigrateAndSeedAsync(db, hasher);
}

app.Run();
