using Microsoft.OpenApi.Models;
using SmsEngine.API.Controllers;
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

// Add logging
builder.Services.AddLogging(configure => configure.AddConsole());

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
