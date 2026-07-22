using System.ComponentModel.DataAnnotations;

namespace BloggingApp.Models
{
    public class Category
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="*The Category name is required.")]
        [MaxLength(100, ErrorMessage ="*Category name cannot exceed 100 characters.")]
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ICollection<Post>? Posts { get; set; }
    }
}
