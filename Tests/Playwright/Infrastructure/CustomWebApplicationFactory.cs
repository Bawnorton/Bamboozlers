using Bamboozlers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.Playwright.Infrastructure;

// I would just like to say thank you to https://github.com/donbavand/playwright-webapplicationfactory for getting me
// out of this headache
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _host;

    public string ServerAddress
    {
        get
        {
            EnsureServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = builder.Build();

        builder.ConfigureWebHost(hostBuilder => hostBuilder.UseKestrel());

        _host = builder.Build();
        _host.Start();
        
        var server = _host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        
        ClientOptions.BaseAddress = addresses!.Addresses.Select(a => new Uri(a)).Last();
        
        testHost.Start();
        return testHost;
    }

    protected override void Dispose(bool disposing)
    {
        _host?.Dispose();
    }
    
    private void EnsureServer()
    {
        if (_host == null)
        {
            using var _ = CreateDefaultClient();
        }
    }
}