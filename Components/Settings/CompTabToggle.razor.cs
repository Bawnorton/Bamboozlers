using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;
public partial class CompTabToggle : CompSettings
{
    protected bool Editing { get; set; }
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
        ChangeViewContext();
        return Task.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        ChangeViewContext();
    }

    protected void ChangeViewContext()
    {
        InteractionMode = Editing ? "edit" : "view";
        ToggleButtonText = Editing ? EditButtonText : ViewButtonText;
        ToggleButtonColor = Editing ? EditButtonColor : ViewButtonColor;
    }
}