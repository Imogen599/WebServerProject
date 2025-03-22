using Microsoft.AspNetCore.Diagnostics;
using MyAuthServer.Services;
using MyAuthServer.SQL;
using Serilog;

SQLDatabaseManager.PrepareDatabases();
var builder = WebApplication.CreateBuilder(args);

// Use a fixed port.
builder.WebHost.UseUrls("https://localhost:5266");

// Add services to the container.
builder.Services.AddScoped<SessionCleanupService>();

builder.Services.AddScoped<IDatabaseServiceFactory, DatabaseServiceFactory>();
builder.Services.AddScoped<ISessionTokenService, SessionTokenService>();
builder.Services.AddScoped<IUserService, UserService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.RequireHttpsAttribute()));

// Serilog initialization.
if (!Directory.Exists("logs"))
    Directory.CreateDirectory("logs");
Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("logs/serverlog-.txt", rollingInterval: RollingInterval.Day).CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handling.
app.UseExceptionHandler(e =>
{
    e.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception != null)
            Log.Error(exception, "Unhandled exception occurred.");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred.");
    });
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
