using System.ComponentModel.DataAnnotations;
using Bamboozlers.Account;

namespace Bamboozlers.Classes.Data;

public abstract class InputModel
{
}

public sealed class EmailInputModel : InputModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "New Email")]
    public string? Email { get; set; }
}

public sealed class UsernameInputModel : InputModel
{
    [Required]
    [ValidUsername(ErrorMessage =
        "Username is invalid. It can only contain letters, numbers, and underscores. There can only be 1 underscore in a row.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";
}

public sealed class PasswordInputModel : InputModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string OldPassword { get; set; } = "";

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
        MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = "";
}

public sealed class DisplayNameInputModel : InputModel
{
    [DataType(DataType.Text)]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = "";
}

public sealed class BioInputModel : InputModel
{
    [DataType(DataType.Text)]
    [StringLength(1000, ErrorMessage = "Description must be less than 1000 characters.")]
    [Display(Name = "Description")]
    public string Bio { get; set; } = "";
}

public sealed class DeleteAccountInputModel : InputModel
{
    [DataType(DataType.Password)] public string Password { get; set; } = "";
}