using HenryTires.Inventory.Api.Extensions;
using HenryTires.Inventory.Api.Middleware;
using HenryTires.Inventory.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to bind to 0.0.0.0 and use PORT from environment (Railway) or default to 5099
var port = Environment.GetEnvironmentVariable("PORT") ?? "5099";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Add custom services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy();

var app = builder.Build();

// Configure Serilog AFTER building the app
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

// Configure middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Henry's Tires Inventory API v1");
    });
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Log.Information("Henry's Tires Inventory API starting on http://0.0.0.0:{Port}", port);
app.Run();
