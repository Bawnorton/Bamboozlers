using Bamboozlers;
using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Blazorise.Icons.FontAwesome;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Bamboozlers.Account;
using Bamboozlers.Classes.Services;
using Bamboozlers.Classes.Services.UserService;
using Blazorise.Bootstrap5;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Client;
using IMessageService = Bamboozlers.Classes.Services.IMessageService;


var builder = WebApplication.CreateBuilder(args);

// Access Configuration from the builder
var configuration = builder.Configuration;

// Add services to the container.
builder.Services
    .AddBlazorise( options => options.Immediate = true )
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IIdentityRedirectManager, IdentityRedirectManager>();
builder.Services.AddScoped<IdentityRedirectManagerWrapper>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IWebSocketService, WebSocketService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("CONNECTION_STRING")));

builder.Services.AddIdentityCore<User>(options =>
                        {
                            options.SignIn.RequireConfirmedAccount = true;
                            options.User.RequireUniqueEmail = true;
                        })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender<User>, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];
    options.InstanceName = "SampleInstance";
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

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();
app.Run();

public partial class Program;