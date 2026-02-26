using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;
using SecureLink.Infrastructure.Helpers;
using SecureLink.Infrastructure.Repositories;
using SecureLink.Infrastructure.Services;

const long maxFileLimit = 5L * 1024 * 1024 * 1024; // 5 GB

var builder = WebApplication.CreateBuilder(args);

var jwtSettings =
    builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "Required configuration section 'JwtSettings' is missing or invalid."
    );

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
            ),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

// File related services.
builder.Services.AddScoped<IFilesService, FilesService>();

// TODO: check on implementing a interface instead of directly injecting validator
builder.Services.AddScoped<FileValidator>();

// TODO: Add an S3 storage service as well.
builder.Services.AddScoped<IFileRepository, LocalStoreRepository>();
builder.Services.Configure<DapperOptions>(builder.Configuration.GetSection("Dapper"));
builder.Services.AddSingleton<IDapperContext, DapperContext>();
builder.Services.AddScoped<FileRepository>();

// User related services
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IUsersValidator, UsersValidator>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();

// Auth related services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();
builder.Services.AddScoped<IAuthValidator, AuthValidator>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxFileLimit + (10 * 1024 * 1024); // 5GB + 10MB buffer
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxFileLimit;
});

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (app.Environment.IsDevelopment() || args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var dapperContext = scope.ServiceProvider.GetRequiredService<IDapperContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<MigrationRunner>>();
    var migrationsPath = Path.Combine(AppContext.BaseDirectory, "Migrations");
    var migrationRunner = new MigrationRunner(dapperContext, logger, migrationsPath);
    await migrationRunner.RunMigrations();

    // In case of production
    if (args.Contains("--migrate"))
        return; // Exit after migrations, don't start the server
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
