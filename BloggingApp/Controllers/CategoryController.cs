using Microsoft.AspNetCore.Mvc;

namespace BloggingApp.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
