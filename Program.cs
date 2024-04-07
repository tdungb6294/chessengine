using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Auth0.AspNetCore.Authentication;
using Chess.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://localhost:5080");

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"] ?? "";
    options.ClientId = builder.Configuration["Auth0:ClientId"] ?? "";
});
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(builder.Configuration["ConnectionStrings:DefaultConnection"] ?? "", new MySqlServerVersion(new Version(8, 3, 0)));
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp =>
        {
            var navMan = sp.GetRequiredService<NavigationManager>();
            return new HubConnectionBuilder()
                .WithUrl(navMan.ToAbsoluteUri(ChessHub.HubUrl))
                .WithAutomaticReconnect()
                .Build();
        });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapHub<ChessHub>(ChessHub.HubUrl);

app.MapGet("/auth/login", async (HttpContext httpContext, string redirectUri = "/") =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(redirectUri)
            .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/auth/logout", async (HttpContext httpContext, string redirectUri = "/") =>
{
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri(redirectUri)
            .Build();

    await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

app.MapPost("/api/gameresults", async (HttpContext context) =>
{
    var dto = await context.Request.ReadFromJsonAsync<CreateGameResultDto>();
    if (dto == null)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Invalid data");
        return;
    }
    var validationResults = new List<ValidationResult>();
    var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);
    if (!isValid)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(validationResults.Select(v => v.ErrorMessage));
        return;
    }
    var gameResult = new GameResult
    {
        PlayerName1 = dto.PlayerName1,
        PlayerName2 = dto.PlayerName2,
        GameStatus = dto.GameStatus,
        CreatedOn = DateTime.UtcNow
    };
    using (var scope = app.Services.CreateScope())
    {
        // Retrieve DbContext from service provider
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.CreateGameResultAsync(gameResult);
    }
    context.Response.StatusCode = StatusCodes.Status201Created;
});


app.MapGet("/api/gameresults", async (HttpContext context) =>
{
    using (var scope = app.Services.CreateScope())
    {
        // Retrieve DbContext from service provider
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Query database for game results
        var gameResults = dbContext.GameResults.ToList();

        // Return game results as JSON
        context.Response.Headers.Append("Content-Type", "application/json");
        await context.Response.WriteAsJsonAsync(gameResults);
    }
});

app.Run();
