using Microsoft.AspNetCore.Components.Authorization;
using TaskManager.Web.Services;
using TaskManager.Web.Services.Auth;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);

// Enable detailed errors
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container.
builder.Services.AddRazorPages();

// Auth
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// HTTP services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddScoped<AuthenticationHandler>();

// Lấy API URL từ cấu hình
var apiUrl = builder.Configuration["ApiUrl"]?.TrimEnd('/') ?? "http://localhost:5200";

// Cấu hình HttpClient với retry policy
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri($"{apiUrl}/");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<AuthenticationHandler>();

builder.Services.AddHttpClient<ITaskService, TaskService>(client =>
{
    client.BaseAddress = new Uri($"{apiUrl}/");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<AuthenticationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ✅ Map đúng cho Blazor Server
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
