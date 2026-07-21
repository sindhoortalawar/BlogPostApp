using BloggingApp.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BloggingApp.Models
{
    public class AppUser : IdentityUser
    {
        [Required(ErrorMessage = "*Full Name is required.")]
        [MinLength(3, ErrorMessage = "User name must be of 3 or more characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Date of Birth is required.")]
        [DataType(DataType.Date), DisplayName("Date Of Birth")]
        [MinimumAge(18, ErrorMessage = "*You must be 18 years or older to create an account.")] // Custom Validation Attribute created for age validation
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage ="*Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "*Profile picture is required.")]
        public string ProfilePictureUrl { get; set; } = string.Empty;

        [ValidateNever]
        public ICollection<Post>? Posts { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;
    }
}
