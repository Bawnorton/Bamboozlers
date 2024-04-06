using Blazorise;
using Blazorise.Modules;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Provider;
using Xunit.Abstractions;

namespace Tests.GroupTests;

public class GroupChatTests : AuthenticatedBlazoriseTestBase
{
    private ITestOutputHelper output;
    public GroupChatTests(ITestOutputHelper outputHelper)
    {
        output = outputHelper;
        //MockDatabaseProvider.SetupMockDbContext();
        Ctx.Services.AddSingleton(new Mock<IJSModalModule>().Object);
        Ctx.Services.AddBlazorise().Replace(ServiceDescriptor.Transient<IComponentActivator, ComponentActivator>());
        
        _ = new MockJsRuntimeProvider(Ctx);
    }
}