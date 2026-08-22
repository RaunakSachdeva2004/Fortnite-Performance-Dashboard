using Microsoft.AspNetCore.Mvc;

namespace FortniteDashboard.Controllers
{
    /// <summary>
    /// Generic production error page. Registered as the fallback target for
    /// app.UseExceptionHandler("/Error") in Program.cs so unhandled exceptions
    /// never leak a stack trace to the user outside Development.
    /// </summary>
    [Route("/Error")]
    public class ErrorController : Controller
    {
        [Route("")]
        public IActionResult Index() => View();
    }
}
