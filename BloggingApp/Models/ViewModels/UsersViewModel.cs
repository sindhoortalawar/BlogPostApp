namespace BloggingApp.Models.ViewModels
{
    public class UsersViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }


        public string Gender { get; set; } = string.Empty;

        public string SelectedRole { get; set; } = string.Empty;


        public string ProfilePictureUrl { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
