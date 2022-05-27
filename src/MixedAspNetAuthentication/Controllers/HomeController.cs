using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MixedAspNetAuthentication.Models;
using System.Diagnostics;

namespace MixedAspNetAuthentication.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> DumpTokens()
        {
            DumpTokenModel model = new DumpTokenModel();

            model.AuthProvider = HttpContext.User.Identity.AuthenticationType;
            model.RefreshToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
            model.AccessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            model.IdToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);

            model.Claims = User.Claims.Select(c => new ClaimDto(c.Type, c.Value)).ToArray();

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}