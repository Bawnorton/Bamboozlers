using AngleSharp.Text;

namespace Bamboozlers.Classes.Interop;

public record KeyData(string key, string code, bool ctrl, bool shift, bool alt, bool meta)
{
    public static KeyData Normal(string key, string code)
    {
        return new KeyData(key, code, false, false, false, false);
    }

    public static KeyData FromChar(char c)
    {
        var key = c.ToString().ToLower();
        var code = "Key" + key.ToUpper();
        return new KeyData(key, code, false, c.IsUppercaseAscii(), false, false);
    }
}