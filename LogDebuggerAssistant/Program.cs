using DevelopmentCommons;
using LogDebuggerAssistant.Components;
using LogDebuggerAssistant.Utilities.Models;
using Microsoft.Extensions.AI;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
.AddInteractiveServerComponents(options =>
{
    options.DetailedErrors = true;
});

builder.Services.AddSingleton<LogParserService>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddSingleton<LLMService>();
builder.Services.AddRadzenComponents();


builder.Services.AddHttpClient();

// Configure the Ollama client
builder.Services.AddHttpClient("Ollama", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
    client.BaseAddress = new Uri("http://localhost:11434");
});

CLogger.Log($"Ollama configuration completed on: {DateTime.Now}");

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});
builder.Services.AddRadzenComponents();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
