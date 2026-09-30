using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.Interfaces;
using TaskTrack.Service.Implementations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text.Json.Serialization;

// Enable legacy timestamp behavior so UTC DateTimes can be mapped to PostgreSQL timestamp without time zone
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercel", policy =>
    {
        var configuredFrontend = builder.Configuration["FRONTEND_URL"];
        policy.SetIsOriginAllowed(origin =>
               Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
               (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(configuredFrontend) && origin.Equals(configuredFrontend, StringComparison.OrdinalIgnoreCase))))
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Configure DbContext from Render's DATABASE_URL or the standard .NET
// ConnectionStrings:DefaultConnection setting (User Secrets in development).
var configuredConnection = builder.Configuration["DATABASE_URL"];
if (string.IsNullOrWhiteSpace(configuredConnection))
{
    configuredConnection = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrWhiteSpace(configuredConnection))
{
    throw new InvalidOperationException(
        "Database connection is not configured. Set DATABASE_URL or ConnectionStrings__DefaultConnection.");
}

var databaseConnection = NormalizeDatabaseConnection(configuredConnection);
builder.Services.AddDbContext<TaskmanagementDbEgrzContext>(options =>
    options.UseNpgsql(databaseConnection));

// Dependency Injection
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITagService, TagService>();

var app = builder.Build();

// Keep Swagger available locally and after deployment for assignment review.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowVercel");

app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

app.Run();

static string NormalizeDatabaseConnection(string connection)
{
    if (!Uri.TryCreate(connection, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
    {
        return connection;
    }

    var credentials = uri.UserInfo.Split(':', 2);
    if (credentials.Length != 2)
    {
        throw new InvalidOperationException("DATABASE_URL does not contain valid credentials.");
    }

    var connectionBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        Username = Uri.UnescapeDataString(credentials[0]),
        Password = Uri.UnescapeDataString(credentials[1]),
        SslMode = SslMode.Require
    };

    return connectionBuilder.ConnectionString;
}
