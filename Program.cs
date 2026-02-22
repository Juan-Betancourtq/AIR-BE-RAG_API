using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Instrumentation.Runtime;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ResumeChat.RagApi.Configuration;
using ResumeChat.RagApi.Hubs;
using ResumeChat.RagApi.Services;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Resume Chat RAG API",
        Version = "v1",
        Description = "AI-powered Resume Chat API using RAG (Retrieval-Augmented Generation) with Azure OpenAI and Azure AI Search",
        Contact = new()
        {
            Name = "Juan Pablo Betancourt",
            Url = new Uri("https://www.betancourtjuanpablo.com")
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.EnableAnnotations();
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://www.betancourtjuanpablo.com",
                "https://betancourtjuanpablo.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add SignalR
builder.Services.AddSignalR();

// Configure Azure services
builder.Services.Configure<AzureOpenAIConfiguration>(
    builder.Configuration.GetSection("AzureOpenAI"));
builder.Services.Configure<AzureSearchConfiguration>(
    builder.Configuration.GetSection("AzureSearch"));

// Register services
builder.Services.AddSingleton<IAzureOpenAIService, AzureOpenAIService>();
builder.Services.AddSingleton<IAzureSearchService, AzureSearchService>();
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddSingleton<ISessionService, SessionService>();

var openTelemetrySection = builder.Configuration.GetSection("OpenTelemetry");
var serviceName = openTelemetrySection["ServiceName"] ?? builder.Environment.ApplicationName;
var serviceVersion = openTelemetrySection["ServiceVersion"]
    ?? typeof(Program).Assembly.GetName().Version?.ToString()
    ?? "1.0.0";
var azureMonitorConnectionString = openTelemetrySection.GetSection("AzureMonitor")["ConnectionString"];

var openTelemetryBuilder = builder.Services.AddOpenTelemetry();

openTelemetryBuilder.ConfigureResource(resource => resource
    .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
    .AddAttributes(new KeyValuePair<string, object>[]
    {
        new("deployment.environment", builder.Environment.EnvironmentName),
        new("service.instance.id", Environment.MachineName)
    }));

openTelemetryBuilder.WithMetrics(metrics =>
{
    metrics.AddAspNetCoreInstrumentation();
    metrics.AddHttpClientInstrumentation();
    metrics.AddRuntimeInstrumentation();
});

openTelemetryBuilder.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation(options =>
    {
        options.RecordException = true;
        options.EnrichWithHttpRequest = (activity, request) =>
        {
            var clientIp = request.HttpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrWhiteSpace(clientIp))
            {
                activity.SetTag("http.client_ip", clientIp);
            }

            var referer = request.Headers.Referer.ToString();
            if (!string.IsNullOrWhiteSpace(referer))
            {
                activity.SetTag("http.request_referer", referer);
            }

            var userAgent = request.Headers.UserAgent.ToString();
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                activity.SetTag("http.user_agent", userAgent);
            }
        };
    });
    tracing.AddHttpClientInstrumentation();
});

if (!string.IsNullOrWhiteSpace(azureMonitorConnectionString))
{
    openTelemetryBuilder.UseAzureMonitor(options =>
    {
        options.ConnectionString = azureMonitorConnectionString;
    });
}

var app = builder.Build();

// JPB: Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resume Chat RAG API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
}
else
{
    // JPB: Enable Swagger in Production for IIS deployment
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resume Chat RAG API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chat-hub");

// JPB: Add a root endpoint to verify API is running
app.MapGet("/", () => Results.Ok(new
{
    message = "Resume Chat RAG API is running",
    version = "v1",
    swagger = "/swagger",
    endpoints = new { chat = "/api/chat", health = "/api/chat/health" }
})).WithTags("Health");

app.Run();
