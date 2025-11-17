using Microsoft.OpenApi.Models;
using Serilog;
using SmsEngine.Application.Interfaces;
using SmsEngine.Infrastructure.Configuration;
using SmsEngine.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SMS Engine API", Version = "v1" });
});

// Configure settings
builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection(SmsSettings.SectionName));

// Register services
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISmsService, SslWirelessSmsService>();

// Serilog configure
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()   
    .WriteTo.File(
        path: "log/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

// ASP.NET Core logging system কে Serilog দিয়ে ব্যবহার করো
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SMS Engine API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();