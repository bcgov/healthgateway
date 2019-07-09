using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HealthGateway.WebClient.Controllers
{
    public class AuthController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Logout()
        {
            return new SignOutResult(new[] { "Cookies", "OpenIdConnect" });
        }
    }
}
