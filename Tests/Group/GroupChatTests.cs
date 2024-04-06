using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Components.Group;
using Bamboozlers.Components.Group.Settings;
using Blazorise;
using Blazorise.Modules;
using Bunit.Extensions.WaitForHelpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Provider;
using Xunit.Abstractions;

namespace Tests.Group;

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