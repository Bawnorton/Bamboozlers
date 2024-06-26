using Bamboozlers;
using Bamboozlers.Account;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Networking.SignalR;
using Bamboozlers.Classes.Services;
using Bamboozlers.Classes.Services.UserServices;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Access Configuration from the builder
var configuration = builder.Configuration;

// Add services to the container.
builder.Services
    .AddBlazorise(options => options.Immediate = true)
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IIdentityRedirectManager, IdentityRedirectManager>();
builder.Services.AddScoped<IdentityRedirectManagerWrapper>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("CONNECTION_STRING"),
        sqlServerOptions => { sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery); });
});

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

builder.Services.AddScoped<ServiceProviderWrapper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserInteractionService, UserInteractionService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IKeyPressService, KeyPressService>();

builder.Services.Configure<IISServerOptions>(options => { options.MaxRequestBodySize = 1024 * 1024 * 100; });
builder.Services.AddSignalR(e => { e.MaximumReceiveMessageSize = 1024 * 1024 * 100; });
builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

string[] urls = builder.Environment.ContentRootPath.Contains("bawnorton")
    ? ["http://192.168.1.199:5152"] // Used to test server when hosted on Ben's local machine as the port forwarding needs to map to an exact IP address.
    : ["http://localhost:5152"];

builder.WebHost.UseUrls(urls);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapHub<BamboozlersHub>(BamboozlersHub.HubUrl, options =>
{
    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();
app.Run();

public partial class Program;