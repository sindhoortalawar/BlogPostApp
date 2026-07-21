using Microsoft.AspNetCore.Mvc;

namespace BloggingApp.Controllers
{
    public class CommentController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }
    }
}
