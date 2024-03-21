namespace Bamboozlers.Classes.Interop;

public record MouseData(string? elementId, bool passed)
{
    public static MouseData Normal()
    {
        return new MouseData(null, true);
    }
}