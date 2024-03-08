
using Blazorise;

namespace Bamboozlers.Components.Settings.EditFields;

/// <class>
/// TabToggle
/// </class>
/// <summary>
/// Abstract class inherited by fields that toggle between "view" and "edit" modes
/// Includes necessary methods that do not need to be repeated in inheriting claes
/// </summary>
public abstract class TabToggle : SettingsComponentBase
{
    private bool Editing { get; set; }
    protected string? InteractionMode { get; set; } = "view";
    protected string EditButtonText { get; set; } = "Cancel";
    protected string ViewButtonText { get; set; } = "Toggle";

    protected Color EditButtonColor { get; set; } = Color.Secondary;
    protected Color ViewButtonColor { get; set; } = Color.Primary;
    protected string? ToggleButtonText { get; set; }
    protected Color? ToggleButtonColor { get; set; }

    protected Task Toggle()
    {
        Editing = !Editing;
        SetViewContext();
        return Task.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        SetViewContext();
    }

    private void SetViewContext()
    {
        InteractionMode = Editing ? "edit" : "view";
        ToggleButtonText = Editing ? EditButtonText : ViewButtonText;
        ToggleButtonColor = Editing ? EditButtonColor : ViewButtonColor;
    }
}