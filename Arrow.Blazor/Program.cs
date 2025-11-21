using Arrow.Blazor.Components;
using Arrow.Blazor.Configuration;
using Arrow.Blazor.Data;
using Arrow.Blazor.Endpoints;
using Arrow.Blazor.Services;
using Arrow.Blazor.Services.Background;
using Arrow.Blazor.Services.Email;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
FeatureToggles.Initialize(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.SlidingExpiration = true;
        options.Cookie.Name = "Arrow.Auth";
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPageViewTracker, PageViewTracker>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IAdminAccessService, AdminAccessService>();
builder.Services.AddHttpClient();
builder.Services.Configure<MailerSendConfiguration>(builder.Configuration.GetSection("MailerSend"));

/// DI stuff
// Email support via MailerSend
builder.Services.AddTransient<IEmailService, MailerSendEmailService>();
// Sample background task that runs daily
builder.Services.AddHostedService<IdleBackgroundService>();
// Background task to cleanup expired password reset tokens (only when email is enabled)
if (FeatureToggles.IsEmailEnabled)
{
    builder.Services.AddHostedService<PasswordResetCleanupService>();
}

builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(Program).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

// If feedback/contact feature is disabled, ensure the `/contact` route returns 404
if (!FeatureToggles.IsFeedbackEnabled)
{
    app.MapGet("/contact", () => Results.NotFound());
}

app.MapRazorComponents<Arrow.Blazor.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapAuthEndpoints();

// Add antiforgery token endpoint for JavaScript
app.MapGet("/antiforgery/token", (Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery, HttpContext context) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions { HttpOnly = false, SameSite = SameSiteMode.Strict });
    return Results.Ok();
});

app.Run();
