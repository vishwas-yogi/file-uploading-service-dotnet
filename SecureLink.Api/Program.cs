using Microsoft.AspNetCore.Http.Features;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;
using SecureLink.Infrastructure.Repositories;
using SecureLink.Infrastructure.Services;

const long maxFileLimit = 5L * 1024 * 1024 * 1024; // 5 GB

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IFileService, FileService>();

// TODO: check on implementing a interface instead of directly injecting validator
builder.Services.AddScoped<FileValidator>();

// TODO: Add an S3 storage service as well.
builder.Services.AddScoped<IFileRepository, LocalStoreRepository>();
builder.Services.Configure<DapperOptions>(builder.Configuration.GetSection("Dapper"));
builder.Services.AddSingleton<IDapperContext, DapperContext>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
