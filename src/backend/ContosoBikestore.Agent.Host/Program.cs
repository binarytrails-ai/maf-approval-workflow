using Azure.AI.Agents.Persistent;
using ContosoBikestore.Agent.Host;
using ContosoBikestore.Agent.Host.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<AppConfig>(sp => new AppConfig(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Set up configuration
var appConfig = new AppConfig(builder.Configuration);
var githubToken = builder.Configuration["GITHUB_TOKEN"]
                  ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
var githubModelsBaseUrl = builder.Configuration["GITHUB_MODELS_BASE_URL"]
                          ?? Environment.GetEnvironmentVariable("GITHUB_MODELS_BASE_URL")
                          ?? "https://models.inference.ai.azure.com";
var githubModelId = builder.Configuration["GITHUB_MODEL_ID"]
                    ?? Environment.GetEnvironmentVariable("GITHUB_MODEL_ID")
                    ?? "gpt-4o";

if (string.IsNullOrWhiteSpace(githubToken))
{
    throw new InvalidOperationException("GitHub Models token not found. Set GITHUB_TOKEN.");
}

var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(githubModelsBaseUrl) };
var openAiClient = new OpenAIClient(new ApiKeyCredential(githubToken), clientOptions);

IChatClient chatClient = openAiClient.GetChatClient(githubModelId)
    .AsIChatClient()
    .AsBuilder()
    .Build();

builder.Services.AddChatClient(chatClient);
builder.Services.AddAGUI();
builder.AddDevUI();

// Add OpenAI services
builder.AddOpenAIChatCompletions();
builder.AddOpenAIResponses();
builder.AddOpenAIConversations();

var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();

var customerServiceAssistant = (await CustomerServiceAssistant.CreateAsync(chatClient, appConfig, jsonOptions))
    .AsBuilder().Build();

builder.Services.AddKeyedSingleton<AIAgent>("CustomerServiceAssistant", customerServiceAssistant);

var app = builder.Build();
app.MapOpenApi();
app.UseCors();

app.MapOpenAIResponses();
app.MapOpenAIConversations();

app.MapOpenAIChatCompletions(customerServiceAssistant);

// Map AGUI endpoint
app.MapAGUI("/agent/customer_service_assistant", customerServiceAssistant);

// Map DevUI - it will discover and use the registered agent
app.MapDevUI();
await app.RunAsync();