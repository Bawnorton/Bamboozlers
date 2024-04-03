using System.Text;

namespace Tests.Helpers;

public class TextHelper
{
    public static string RandomText(int length = 500, int newLineChance = 3)
    {
        var random = new Random();
        var text = new StringBuilder();
        for (var i = 0; i < length; i++)
        {
            text.Append((char)random.Next(32, 127));
            if (random.Next(0, 100) < newLineChance) text.Append('\n');
        }

        var textString = text.ToString();
        while (textString.StartsWith('\n')) textString = textString[1..];
        while (textString.EndsWith('\n')) textString = textString[..^1];
        return textString;
    }
}