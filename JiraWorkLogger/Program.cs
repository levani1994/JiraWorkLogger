using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using JiraWorkLogger.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<WorkLogService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Only use HTTPS redirection in non-container environments
// Render.com handles HTTPS termination
var port = Environment.GetEnvironmentVariable("PORT");
if (string.IsNullOrEmpty(port))
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Use PORT environment variable if available (for Render.com)
var url = string.IsNullOrEmpty(port) ? null : $"http://0.0.0.0:{port}";
if (url != null)
{
    app.Urls.Add(url);
}

app.Run();
