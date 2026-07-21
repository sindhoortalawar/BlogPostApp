using Microsoft.EntityFrameworkCore;
using BloggingApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BloggingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Post> Posts { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seeding initial data to Category table
            modelBuilder.Entity<Category>().HasData(
                new Category {  Id=1, CategoryName="Technology"},
                new Category { Id=2, CategoryName="Health"},
                new Category { Id=3, CategoryName="LifeStyle"}
            );

            // Seeding initial data to Post table
            //modelBuilder.Entity<Post>().HasData(
            //    new Post
            //    {
            //        Id = 1,
            //        Title = "Tech Post 1",
            //        Content = "Content of Tech Post 1",
            //        Author = "John Doe",
            //        PublishedDate = new DateTime(2026, 7, 2), // Static date instead of DateTime.Now
            //        CategoryId = 1,
            //        FeatureImagePath = "tech_image.jpg", // Sample image path
            //    },
            //    new Post
            //    {
            //        Id = 2,
            //        Title = "Health Post 1",
            //        Content = "Content of Health Post 1",
            //        Author = "Jane Doe",
            //        PublishedDate = new DateTime(2026, 7, 2), // Static date
            //        CategoryId = 2,
            //        FeatureImagePath = "health_image.jpg", // Sample image path
            //    },
            //    new Post
            //    {
            //        Id = 3,
            //        Title = "Lifestyle Post 1",
            //        Content = "Content of Lifestyle Post 1",
            //        Author = "Alex Smith",
            //        PublishedDate = new DateTime(2026, 7, 2), // Static date
            //        CategoryId = 3,
            //        FeatureImagePath = "lifestyle_image.jpg", // Sample image path
            //    }
            //);
        }
    }
}
