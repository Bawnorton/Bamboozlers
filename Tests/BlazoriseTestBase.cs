using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Tests.bUnit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tests;

public class BlazoriseTestBase : TestBase
{
    protected BlazoriseTestBase()
    {
        Ctx.Services
            .Replace(ServiceDescriptor.Transient<IComponentActivator, ComponentActivator>())
            .AddBlazoriseTests()
            .AddClassProvider(() => new Bootstrap5ClassProvider())
            .AddStyleProvider(() => new Bootstrap5StyleProvider())
            .AddEmptyIconProvider();

        Ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        Ctx.JSInterop
            .AddBlazoriseBreakpoint()
            .AddBlazoriseButton()
            .AddBlazoriseClosable()
            .AddBlazoriseDropdown()
            .AddBlazoriseModal()
            .AddBlazoriseTable()
            .AddBlazoriseUtilities()
            .AddBlazoriseDataGrid()
            .AddBlazoriseDatePicker()
            .AddBlazoriseDragDrop()
            .AddBlazoriseNumericEdit()
            .AddBlazoriseTextEdit();
    }
}