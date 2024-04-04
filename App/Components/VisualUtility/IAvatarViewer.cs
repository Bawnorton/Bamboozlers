namespace Bamboozlers.Components.VisualUtility;

public interface IAvatarViewer
{
    public int DisplayPx { get; }
    public byte[]? Avatar { get; }
    public string? DefaultSrc { get; }

    public string GetDisplayString()
    {
        return Avatar is not null
            ? $"data:image/png;base64,{Convert.ToBase64String(Avatar)}"
            : DefaultSrc!;
    }
}