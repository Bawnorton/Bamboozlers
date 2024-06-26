using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Bamboozlers.Account;

public class ValidUsernameAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null || string.IsNullOrEmpty(value.ToString()))
            return false;

        var username = value.ToString();
        // Regex pattern to match your criteria
        var regex = new Regex(@"^(?:_?[a-zA-Z0-9]+)*_?$");
        return username is not null && regex.IsMatch(username);
    }
}