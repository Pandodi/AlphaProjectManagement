using System.ComponentModel.DataAnnotations;

namespace Presentation_WebApp.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Required")]
    [Display(Name = "Email", Prompt = "Your email address")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Required")]
    [Display(Name = "Password", Prompt = "Enter your password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}
