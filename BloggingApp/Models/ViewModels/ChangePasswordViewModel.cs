using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BloggingApp.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "*Password is required.")]
        [DataType(DataType.Password)]
        //[MinLength(8, ErrorMessage = "**Password must be of minimum 8 characters long."), MaxLength(15, ErrorMessage = "**Password should be maximum 15 characters long.")]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,15}$", ErrorMessage = "**Password must be of minimum 8 characters long, must contain one lowecase letter, one uppercase letter, one digit and one special character.")]
        public string OldPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "*Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "**Password must be of minimum 8 characters long."), MaxLength(15, ErrorMessage = "**Password should be maximum 15 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,15}$", ErrorMessage = "**Password must be of minimum 8 characters long, must contain one lowecase letter, one uppercase letter, one digit and one special character.")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "*Password does not match the confirm password.")]
        [DisplayName("Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
