using Blazored.LocalStorage;
using LeaveFlow.BlazorUI.Components;
using LeaveFlow.BlazorUI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HTTP client pointing at the API
builder.Services.AddHttpClient<ApiService>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
        ?? "https://localhost:5001/"));

// Blazored LocalStorage for JWT persistence
builder.Services.AddBlazoredLocalStorage();

// App services
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
