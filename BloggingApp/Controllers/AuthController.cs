using BloggingApp.Data;
using BloggingApp.Models;
using BloggingApp.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace BloggingApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        //private readonly ApplicationDbContext _context; // Not required as we are using Identity with custom User class for Authentication purpose
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        private readonly string[] allowedExtensions = [".jpg", ".jpeg", ".png"];

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
            _logger.LogInformation("Login attempt initiated for user: {Email}", loginModel.Email); // Structured Logging

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Authentication failed: User with email {Email} not found.", loginModel.Email);
                ModelState.AddModelError("", "Failed to Login. Please try again.");
                return View(loginModel);
            }

            // Validate Email - User with the given email is exists in the DB or not
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user == null) return View(loginModel);

            var signInResult = await _signInManager.PasswordSignInAsync(user, loginModel.Password, isPersistent: false, lockoutOnFailure: false);
            // Check user is valid and Validate password
            if (!signInResult.Succeeded)
            {
                _logger.LogInformation("Authentication failed: Invalid credentials : {Email}", loginModel.Email); // Structured Logging
                ModelState.AddModelError("", "Authentication failed: Invalid credentials. Please try again.");
                return View(loginModel);
            }

            //var claims = new List<Claim>
            //{
            //    new Claim("UserName", user.Email),
            //    new Claim(ClaimTypes.Email, user.Email),
            //    new Claim("FullName", user.FullName),
            //};

            HttpContext.Session.SetString("FullName", user.FullName);
            
            _logger.LogInformation("Authentication Successful : User with email {Email} logged in successfully.", loginModel.Email);
            return RedirectToAction("Index", "Post");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Clear any session memory or standard Identity cookie residues if present
            HttpContext.Session.Clear();
            
            await _signInManager.SignOutAsync();

            _logger.LogInformation("Logout successful : Redirecting to Login page.");
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var roles = await GetRolesList(null);
            ViewBag.RolesList = new SelectList(roles, "Name", "Name");

            return View();
        }


        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(RegisterViewModel registerModel)
        {
            _logger.LogInformation("Registration attempt initiated for user: {Email}", registerModel.Email); // Structured Logging

            if (!ModelState.IsValid)
            {
                var roles = await GetRolesList(null); 
                ViewBag.RolesList = new SelectList(roles, "Name", "Name");
                return View(registerModel);
            }
            if(registerModel.ProfilePicture != null)
            {
                var inputFileExtension = Path.GetExtension(registerModel.ProfilePicture.FileName).ToLower();

                bool isAllowed = allowedExtensions.Contains(inputFileExtension);

                if (!isAllowed)
                {
                    _logger.LogWarning("Regiistration Failed : Failed to register user with {Email} : Invalid file uploaded.", registerModel.Email);
                    ModelState.AddModelError(string.Empty, "Invalid file uploaded. Images with .jpg, .jpeg, .png extensions are allowed.");
                    return View(registerModel);
                }

                var dbFilePath = await UploadFileFolder(registerModel.ProfilePicture);

                var user = new AppUser
                {
                    FullName = registerModel.FullName,
                    UserName = registerModel.Email,
                    Email = registerModel.Email,
                    PhoneNumber = registerModel.PhoneNumber,
                    Gender = registerModel.Gender,
                    DateOfBirth = DateOnly.FromDateTime(registerModel.DateOfBirth),
                    Address = registerModel.Address,
                    ProfilePictureUrl = dbFilePath
                };

                var result = await _userManager.CreateAsync(user, registerModel.Password);

                if(result.Succeeded)
                {
                    if(!await _roleManager.RoleExistsAsync(registerModel.SelectedRole))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(registerModel.SelectedRole));
                    }

                    await _userManager.AddToRoleAsync(user, registerModel.SelectedRole);

                    _logger.LogInformation("Registration Successful : User with email {Email} registered successfully.", registerModel.Email);

                   // Optional - Signin the user after successful registration
                   await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Post");
                }

                foreach(var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            _logger.LogError("Registration Failed : User with {Email} failed to register.", registerModel.Email);
            return View(registerModel);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            //GetRolesList();

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return BadRequest();

            // Get user role
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();


            var userProfile = new ProfileViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                ProfilePictureUrl = user.ProfilePictureUrl,
                SelectedRole = role,

            };
            return View(userProfile);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return BadRequest();

            //GetRolesList();
            var roles = await GetRolesList(_userManager.GetRolesAsync(user).ToString());
            ViewBag.RolesList = new SelectList(roles, "Name", "Name");


            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            var userProfile = new EditProfileViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Address = user.Address,
                SelectedRole = role 
            };

            return View(userProfile);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProfile(EditProfileViewModel editProfileModel)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.Id != editProfileModel.UserId) return NotFound();

            if (!ModelState.IsValid)
            {
                var roles = await GetRolesList(_userManager.GetRolesAsync(user).ToString());
                ViewBag.RolesList = new SelectList(roles, "Name", "Name");
                return View(editProfileModel);
            }


            //user.Id = editProfileModel.UserId;
            user.FullName = editProfileModel.FullName;
            user.Email = editProfileModel.Email;
            user.Gender = editProfileModel.Gender;
            user.PhoneNumber = editProfileModel.PhoneNumber;
            user.Address = editProfileModel.Address;
            user.DateOfBirth = editProfileModel.DateOfBirth;

            // Check whether the user has uploaded new Profile picture
            if(editProfileModel.ProfilePicture?.FileName != null)
            {
                // If new profile picture is uploaded, Then validate extension of the file
                var userFileExtension = Path.GetExtension(editProfileModel.ProfilePicture.FileName);

                if(!allowedExtensions.Contains(userFileExtension))
                {
                    ModelState.AddModelError("", "*Invalid File uploaded.");
                    return View(editProfileModel);
                }

                // If the uploaded file is having valid extension then proceed to delete the existing profile picture 
                var existingFile = Path.Combine(_webHostEnvironment.WebRootPath, "profiles",Path.GetExtension(user.ProfilePictureUrl));

                // Delete the existing file
                if (!string.IsNullOrEmpty(existingFile)) System.IO.File.Delete(existingFile);

                // Save the new file url in the DB
                user.ProfilePictureUrl = await UploadFileFolder(editProfileModel.ProfilePicture);
            }

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "User details updated successfully.";
            return RedirectToAction("Profile", "Auth");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {

            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordModel)
        {

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return BadRequest();

            var result = await _userManager.ChangePasswordAsync(user, changePasswordModel.OldPassword, changePasswordModel.NewPassword);

            if(result.Succeeded)
            {
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction("Profile", "Auth");
            }

            return View(changePasswordModel);
        }


        private async Task<List<IdentityRole>> GetRolesList(string? userRole)
        {
            List<IdentityRole> roles;
            if (userRole != null && userRole.Equals("Admin"))
                roles = await _roleManager.Roles.ToListAsync();
            else
                roles = await _roleManager.Roles.Where(r => r.Name != "Admin" && r.Name != "SuperAdmin" && r.Name != "Manager").ToListAsync();

            //if (roles == null) return null;
            //ViewBag.RolesList = new SelectList(roles, "Name", "Name");
            return roles;
        }

        private async Task<string> UploadFileFolder(IFormFile profileImage)
        {
            var inputFileExtension = Path.GetExtension(profileImage.FileName).ToLower();

            var fileName = Guid.NewGuid().ToString() + inputFileExtension;

            var wwwWebRootPath = _webHostEnvironment.WebRootPath;

            var imagesFolderPath = Path.Combine(wwwWebRootPath, "profiles");

            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            var filePath = Path.Combine(imagesFolderPath, fileName);

            try
            {
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }
            }
            catch(Exception ex)
            {
                return "Error : File uploading error." + ex.Message;
            }

            return "/profiles/" + fileName;

        }

    }
}
