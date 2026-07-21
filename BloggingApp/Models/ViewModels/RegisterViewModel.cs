using BloggingApp.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BloggingApp.Models.ViewModels
{
    public class RegisterViewModel
    {
        //public AppUser User { get; set; }


        [Required(ErrorMessage = "*Full Name is required.")]
        [MinLength(3, ErrorMessage = "User name must be of 3 or more characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Email is required.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "*Invalid Email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Phone number is required."), DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[6-9][0-9]{9}$", ErrorMessage = "*Invalid Phone Number"), DisplayName("Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "**Password must be of minimum 8 characters long."), MaxLength(15, ErrorMessage = "**Password should be maximum 15 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,15}$", ErrorMessage = "**Password must be of minimum 8 characters long, must contain one lowecase letter, one uppercase letter, one digit and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "*Password does not match the confirm password.")]
        [DisplayName("Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "*Date of Birth is required.")]
        [DataType(DataType.Date), DisplayName("Date Of Birth")]
        [MinimumAge(18, ErrorMessage = "You must be 18 years or older to create an account.")] // Custom Validation Attribute created for age validation
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "*Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Role is required.")]
        [DisplayName("User Role")]
        public string SelectedRole { get; set; } = string.Empty;

        [ValidateNever]
        public IFormFile? ProfilePicture { get; set; }

        [Required(ErrorMessage = "*Address is required")]
        public string Address { get; set; } = string.Empty;
    }
}
