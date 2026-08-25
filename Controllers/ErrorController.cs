using Microsoft.AspNetCore.Mvc;

namespace FortniteDashboard.Controllers
{
    [Route("/Error")]
    public class ErrorController : Controller
    {
        [Route("")]
        public IActionResult Index() => View();

        [Route("404")]
        public IActionResult NotFoundPage() => View("NotFound");

        [Route("403")]
        public IActionResult AccessDeniedPage() => View("AccessDenied");
    }
}
