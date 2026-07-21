using System.ComponentModel.DataAnnotations;

namespace BloggingApp.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "*Email is required.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Email is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

    }
}
