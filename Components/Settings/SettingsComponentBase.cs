/* BASE CLASS FOR SETTINGS COMPONENTS */

using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Data.ViewModel;
using Bamboozlers.Classes.Services;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public class SettingsComponentBase : ComponentBase
{
    [Parameter]
    public EventCallback<AlertArguments> AlertEventCallback { get; init; }
    
    [Parameter]
    public EventCallback StateChangedCallback { get; init; }

    [Parameter]
    public Func<UserDataRecord, Task<bool>>? DataChangeFunction { get; init; }
}