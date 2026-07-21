using BloggingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloggingApp.ViewComponents
{
    [ViewComponent(Name = "NavbarUser")]
    public class NavbarUserViewComponent : ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public NavbarUserViewComponent(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_signInManager.IsSignedIn(UserClaimsPrincipal))
                return View((AppUser?)null);

            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);

            return View(user);
        }


    }

    //public class NavabarViewModel
    //{
    //    public bool IsAuthenticated { get; set; }

    //    public string FullName { get; set; } = string.Empty;

    //    public string Email { get; set; } = string.Empty;

    //    public string ProfilePictureUrl { get; set; } = string.Empty;
    //}
}
