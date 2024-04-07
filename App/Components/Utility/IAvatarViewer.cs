using Bunit.Extensions;

namespace Bamboozlers.Components.Utility;

public interface IAvatarViewer
{
    public int DisplayPx { get; }
    public byte[]? Avatar { get; }
    public string? DefaultSrc { get; }

    public string GetDisplayString();
}