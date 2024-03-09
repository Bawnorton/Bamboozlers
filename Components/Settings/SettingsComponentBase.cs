/* BASE CLASS FOR SETTINGS COMPONENTS */

using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public class SettingsComponentBase : ComponentBase
{
    [Parameter]
    public EventCallback<StatusArguments> StatusChangeEvent { get; init; }

    [Parameter]
    public Func<UserDataRecord, Task<bool>>? DataChangeFunction { get; init; }
}

public sealed class StatusArguments(Color statusColor, bool statusVisible, string statusMessage, string statusDescription)
{
    public static readonly StatusArguments BasicStatusArgs = new (
        Color.Danger,
        true,
        "Could not perform the desired operation.",
        "An unexpected error occurred."
    );

    public StatusArguments() : this(Color.Default, false, "", "") { }

    public readonly Color StatusColor = statusColor;
    public readonly bool StatusVisible = statusVisible;
    public readonly string StatusMessage = statusMessage;
    public readonly string StatusDescription = statusDescription;
}