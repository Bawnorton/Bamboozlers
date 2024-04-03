/* BASE CLASS FOR SETTINGS COMPONENTS */

using Bamboozlers.Classes.Data;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public class SettingsComponentBase : UserViewComponentBase
{
    [Parameter] public EventCallback<AlertArguments> AlertEventCallback { get; init; }

    [Parameter] public Func<UserDataRecord, Task<bool>>? DataChangeFunction { get; init; }
}