using Azure.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAI.API.Middleware;
using ChatbotAI.Application.Common.Behaviors;
using ChatbotAI.Persistence;
using System.Reflection;
using ChatbotAI.Application.Commands.Chat.RateAiMessage;
using ChatbotAI.Application.Interfaces;
using ChatbotAI.Application.Queries.StreamAiResponse;
using ChatbotAI.Infrastructure.FakeAI;
using ChatbotAI.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);
 
    // KeyVault
    /*builder.Configuration.AddAzureKeyVault(
        new Uri($"https://ChatbotAI-kv.vault.azure.net/"),
        new DefaultAzureCredential()); */
    
    // MediatR
    builder.Services.AddScoped<IChatRepository, ChatRepository>();
    builder.Services.AddScoped<IAiResponder, FakeAiResponder>();
     
    builder.Services.AddMediatR(typeof(RateAiMessageCommandHandler).Assembly);
    builder.Services.AddValidatorsFromAssembly(typeof(RateAiMessageCommandValidator).Assembly);

    builder.Services.AddMediatR(typeof(StreamAiResponseQueryHandler).Assembly);
    builder.Services.AddValidatorsFromAssembly(typeof(StreamAiResponseQueryValidator).Assembly);
    
    // FluentValidation
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(80); // Azure Container App
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

//app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseMiddleware<ExceptionHandlingMiddleware>();
 
app.Run();
