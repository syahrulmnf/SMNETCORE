using Microsoft.AspNetCore.Mvc;

namespace SMNETCORE.ADONSAPI.Controllers
{
    public class ProjectsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
