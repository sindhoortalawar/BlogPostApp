using BloggingApp.Data;
using BloggingApp.Models;
using BloggingApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BloggingApp.Controllers
{
    public class PostController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager, ILogger<PostController> logger) : Controller
    {
        private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png"];


        [HttpGet]
        public IActionResult Index(int? categoryId)
        {
            var postQuery = context.Posts.Include(p => p.Category).AsQueryable();
            if(categoryId.HasValue)
            {
                postQuery = postQuery.Where(p => p.CategoryId == categoryId);
            }

            var posts = postQuery.ToList();

            ViewBag.Categories = context.Categories.ToList();

            return View(posts);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, PostOwner")]
        public IActionResult Create()
        {
            var postViewModel = new PostViewModel();
            postViewModel.Categories = context.Categories.Select(c =>
                new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CategoryName
                }
            ).ToList();

            return View(postViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, PostOwner")]
        public async Task<IActionResult> Create(PostViewModel postViewModel)
        {
            if (ModelState.IsValid)
            {
                var inputFileExtension = Path.GetExtension(postViewModel.FeatureImage.FileName).ToLower();

                bool isAllowed = _allowedExtensions.Contains(inputFileExtension);

                if (!isAllowed)
                {
                    ModelState.AddModelError("", "Invalid image format. Allowed formats are .jpg, .jpeg, .png");

                    return View(postViewModel);
                }

                //Assign user id to the newly created post
                
                postViewModel.Post.FeatureImagePath = await UploadFileFolder(postViewModel.FeatureImage);

                await context.Posts.AddAsync(postViewModel.Post);

                await context.SaveChangesAsync();

                logger.LogInformation("A Blog post has been created by the user");
                TempData["Success"] = "New Blogpost created successfully.";
                return RedirectToAction("MyBlogs");
            }

            postViewModel.Categories = context.Categories.Select(c =>
            new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CategoryName
                }
            ).ToList();

            return View(postViewModel);

        }


        [HttpPost]
        [Authorize(Roles = "Admin, PostOwner")]
        public async Task<IActionResult> Edit(EditPostViewModel editPostViewModel) 
        {
            if (editPostViewModel == null) return NotFound();
            var user = await userManager.GetUserAsync(User);

            if(user?.Id != editPostViewModel.Post.UserId)
            {
                ModelState.AddModelError("", "**You are not Authorized to perform this operation.");
            }
            if(!ModelState.IsValid)
            {
                editPostViewModel.Categories = context.Categories.Select(c =>
                    new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.CategoryName
                    }
                ).ToList();

                return View(editPostViewModel);
            }
            if (editPostViewModel.FeatureImage != null)
            {
                var inputFileExtension = Path.GetExtension(editPostViewModel.FeatureImage.FileName).ToLower();

                bool isAllowed = _allowedExtensions.Contains(inputFileExtension);

                if (!isAllowed)
                {
                    ModelState.AddModelError("", "Invalid image format. Allowed formats are .jpg, .jpeg, .png");
                    logger.LogError("Error : Invalid image format. Failed to update blog post details by the user");
                    TempData["Error"] = "Failed to update blogpost details. Please try again.";
                    return View(editPostViewModel);
                }

                var existingFile = await context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == editPostViewModel.Post.Id);

                var oldFilePath = Path.Combine(webHostEnvironment.WebRootPath, "images", Path.GetFileName(existingFile.FeatureImagePath));

                if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);

                editPostViewModel.Post.FeatureImagePath = await UploadFileFolder(editPostViewModel.FeatureImage);
            }
            else
            {
                var existingPost = await context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == editPostViewModel.Post.Id);

                if (existingPost == null) return NotFound();

                editPostViewModel.Post.FeatureImagePath = existingPost.FeatureImagePath;
            }

            context.Posts.Update(editPostViewModel.Post);

            await context.SaveChangesAsync();

            logger.LogInformation("The Blog post details have been updated by the user.");
            TempData["Success"] = "Blogpost details updated successfully.";
            return RedirectToAction("Details", "Post", new { id = editPostViewModel.Post.Id });

        }

        
        public async Task<string> UploadFileFolder(IFormFile file)
        {
            var inputFileExtension = Path.GetExtension(file.FileName);

            var fileName = Guid.NewGuid().ToString() + inputFileExtension;

            var wwwRootPath = webHostEnvironment.WebRootPath;

            var imagesFolderPath = Path.Combine(wwwRootPath, "images");

            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            var filePath = Path.Combine(imagesFolderPath, fileName);

            try
            {
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                return "Error : File uploading error." + ex.Message;
            }

            return "/images/" + fileName;

        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var post = await context.Posts.Include(c => c.Category).Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        
        public async Task<JsonResult> AddComment([FromBody]Comment comment)
        {
            var user = await userManager.GetUserAsync(User);

            comment.Username = user.FullName;
            comment.CommentDate = DateTime.UtcNow;

            context.Comments.Add(comment);

            context.SaveChanges();

            return Json(
                new
                {
                    username = user.FullName,
                    commentDate = comment.CommentDate.ToString("MMMM dd, yyyy"),
                    content = comment.Content,
                }
            );
        }

        [HttpGet]
        [Authorize(Roles = "Admin, PostOwner")]
        public async Task<IActionResult> Edit(int id)
        {
            var postFromDb = await context.Posts.Include(c => c.Category).Include(c => c.Comments).FirstOrDefaultAsync(p => p.Id == id);

            if (postFromDb == null) return NotFound();

            var postViewModel = new EditPostViewModel {
                Post = postFromDb,
                Categories = context.Categories.Select(c =>
                    new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.CategoryName
                    }
                ).ToList()
            };

            return View(postViewModel);
        }


        [HttpGet]
        [Authorize(Roles = "Admin, PostOwner")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await context.Posts.Include(c => c.Category).FirstOrDefaultAsync(p => p.Id == id);


            if (post == null)
            {
                return NotFound();

            }

            return View(post);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, PostOwner")]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            if (id <= 0) return BadRequest();

            var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            // Check for the image of post to be deleted and delete the corresponding image file from the folder
            if(!string.IsNullOrEmpty(post.FeatureImagePath))
            {
                var imagePath = Path.Combine(webHostEnvironment.WebRootPath, "images", Path.GetFileName(post.FeatureImagePath));
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
            }

            // Delete the post from DB
            context.Posts.Remove(post);
            // Save the changes
            await context.SaveChangesAsync();

            logger.LogInformation("The Blog post has been deleted by the user");
            TempData["Success"] = "The Blogpost deleted successfully.";
            return RedirectToAction("Index", "Post");
        }
    
        [HttpGet]
        [Authorize(Roles = "Admin, PostOwner")]
        public async Task<IActionResult> MyBlogs(int? categoryId)
        { 
            var userId = userManager.GetUserId(User);

            var postQuery =  context.Posts.Include(p => p.Category).AsQueryable().Where(p => p.UserId == userId);
            if (categoryId.HasValue)
            {
                postQuery = postQuery.Where(p => p.CategoryId == categoryId);
            }

            var posts = postQuery.ToList();

            ViewBag.Categories = context.Categories.ToList();


            //var myPosts = await context.Posts.Where(p => p.UserId == userId).ToListAsync();

            return View(posts);
        }


    }
}
