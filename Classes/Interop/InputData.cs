namespace Bamboozlers.Classes.Interop;

public record InputData(string? elementId, string key, string code, bool ctrl, bool shift, bool alt, bool meta, string content, bool passed)
{
    public static InputData Normal(string key, string code)
    {
        return new InputData(null, key, code, false, false, false, false, key, true);
    }
}