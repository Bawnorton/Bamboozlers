using Blazorise;

namespace Bamboozlers.Classes.Data;

/// <summary>
///     Record used to pass information about actions between components, used primarily in Components/Settings
/// </summary>
/// <param name="statusColor">The color the Blazorise alert component should have.</param>
/// <param name="statusVisible">Whether the Blazorise alert component should be visible or not.</param>
/// <param name="statusMessage">The main message of the alert.</param>
/// <param name="statusDescription">The further description of the alert.</param>
public sealed record AlertArguments(
    Color? AlertColor = null,
    bool AlertVisible = false,
    string AlertMessage = "",
    string AlertDescription = "")
{
    /// <summary>
    ///     A generic Alert Arguments instance to be reused.
    /// </summary>
    public static readonly AlertArguments DefaultErrorAlertArgs = new(
        Color.Danger,
        true,
        "Could not perform the desired operation.",
        "An unexpected error occurred."
    );

    public Color AlertColor { get; } = AlertColor ?? Color.Default;
    public bool AlertVisible { get; } = AlertVisible;
    public string AlertMessage { get; } = AlertMessage;
    public string AlertDescription { get; } = AlertDescription;
}