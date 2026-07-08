using Microsoft.AspNetCore.Mvc;

namespace SMNETCORE.ADONSAPI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
