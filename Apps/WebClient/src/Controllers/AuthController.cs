using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HealthGateway.WebClient.Controllers
{
    public class AuthController : Controller
    {
            private readonly IConfiguration _config;
            private readonly ILogger _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _config = configuration;
            _logger = logger;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logon()
        {
            _logger.LogTrace("Getting Bearer token");
            var user = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var token = await HttpContext.GetTokenAsync("access_token");
            return new OkObjectResult(new LogonResult{ Auth=true,Token=token,User=user});
        }

        public IActionResult Logout()
        {
            _logger.LogTrace("Logging user out");
            var props = new AuthenticationProperties(){RedirectUri = _config["OIDC:LogoffURI "]};
            return new SignOutResult(new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme },props);
        }
    }

    public class LogonResult
    {
        public bool Auth {get; set;}
        public string Token {get; set;}
        public string User {get; set;}

        public LogonResult()
        {
        }
    }
}
