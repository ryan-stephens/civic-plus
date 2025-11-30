using CivicPlusCalendar.Api.Configuration;
using CivicPlusCalendar.Api.Services;
using Microsoft.Extensions.Options;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure CivicPlus API settings
builder.Services.Configure<CivicPlusApiSettings>(
    builder.Configuration.GetSection("CivicPlusApi"));

// Register HttpClient and CivicPlus Services
// Auth service handles authentication logic
builder.Services.AddHttpClient<ICivicPlusAuthService, CivicPlusAuthService>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<CivicPlusApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
});

// API service handles event operations and depends on auth service
builder.Services.AddHttpClient<ICivicPlusApiService, CivicPlusApiService>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<CivicPlusApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
});

// Add CORS policy for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Include XML comments for better documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CivicPlus Calendar API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAngularApp");

app.MapControllers();

app.Run();
