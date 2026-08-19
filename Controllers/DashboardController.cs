using Microsoft.AspNetCore.Mvc;

namespace FortniteDashboard.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
