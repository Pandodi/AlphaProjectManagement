using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Presentation_WebApp.ViewModels;

public class AddMemberViewModel
{
    
    [Display(Name = "Member Image", Prompt = "Select a image")]
    [DataType(DataType.Upload)]
    public IFormFile? MemberImage { get; set; }
    public string? UserImage { get; set; }

    [Display(Name = "First Name", Prompt = "Enter first name")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string FirstName { get; set; } = null!;

    [Display(Name = "Last Name", Prompt = "Enter last name")]
    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Required")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Member Email", Prompt = "Enter member email")]
    [DataType(DataType.EmailAddress)]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
    [Required(ErrorMessage = "Required")]
    public string Email { get; set; } = null!;

    [Display(Name = "Phone", Prompt = "Enter phone number")]
    [RegularExpression(@"^\+?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Please enter a valid phone number.")]
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Job Title", Prompt = "Enter job title")]
    [DataType(DataType.Text)]
    public string? JobTitle { get; set; }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    public AddMemberAddressViewModel Address { get; set; } = new();

}

public class AddMemberAddressViewModel
{
    [Display(Name = "Street Name", Prompt = "Enter street name")]
    [DataType(DataType.Text)]
    public string? StreetName { get; set; }

    [Display(Name = "Postal Code", Prompt = "Enter postal code")]
    [DataType(DataType.PostalCode)]
    public string? PostalCode { get; set; }

    [Display(Name = "City", Prompt = "Enter city")]
    [DataType(DataType.Text)]
    public string? City { get; set; }
}
