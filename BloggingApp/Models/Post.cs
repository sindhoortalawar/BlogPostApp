using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloggingApp.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "*Title is required."), MaxLength(200, ErrorMessage = "*Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Author name is required."), MaxLength(100, ErrorMessage ="*Author name cannot exceed 100 characters"), MinLength(3, ErrorMessage ="*Author name must be of 3 or more characters")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Content is required.")]
        public string Content { get; set; } = string.Empty;

        //To store only date in db and not the time
        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; } 

        [ValidateNever]
        public string UserId { get; set; } = string.Empty;
        [ValidateNever]
        [ForeignKey("UserId")]
        public AppUser AppUser { get; set; }

        [ValidateNever]
        public string FeatureImagePath { get; set; } = string.Empty;

        [ValidateNever]
        public ICollection<Comment> Comments { get; set; } 

        [Required]
        public bool IsActive { get; set; } = true;
    }
}
