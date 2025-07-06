using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAI.API.Middleware;
using ChatbotAI.Persistence;
using ChatbotAI.Persistence.Repositories;
using ChatbotAI.Application.Commands.Chat.RateAiMessage;
using ChatbotAI.Application.Interfaces;
using ChatbotAI.Application.Queries.Chat.StreamAiResponse;
using ChatbotAI.Infrastructure.FakeAI;

var builder = WebApplication.CreateBuilder(args);
 
// Repositories and Services
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IAiResponder, FakeAiResponder>();

builder.Services.AddMediatR(typeof(RateAiMessageCommandHandler).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(RateAiMessageCommandValidator).Assembly);

builder.Services.AddMediatR(typeof(StreamAiResponseQueryHandler).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(StreamAiResponseQueryValidator).Assembly);

// Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ChatbotAI API",
        Version = "v1"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowedOrigins != null)
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
    });
});

// Database
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Kestrel port configuration for Azure
if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(80);
    });
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatbotAI API v1");
});

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization(); 

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();
