using SecureLink.Contracts;
using SecureLink.Repositories;
using SecureLink.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// TODO: check on implementing a interface instead of directly injecting validator
builder.Services.AddScoped<FileUploadValidator>();

// TODO: Add an S3 storage service as well.
builder.Services.AddScoped<IFileUploadRepository, LocalStoreRepository>();

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
