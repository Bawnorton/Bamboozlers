namespace Bamboozlers.Components.Settings.EditFields;

public abstract class EditField : SettingsComponentBase
{
    public virtual async Task OnValidSubmitAsync()
    {
        return;
    }
}