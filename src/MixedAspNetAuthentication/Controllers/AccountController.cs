using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MixedAspNetAuthentication.Models.Account;
using System.Security.Claims;

namespace MixedAspNetAuthentication.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> PerformLogin(LoginModel loginModel) 
        {
            //now you should check the user
            var claims = new List<Claim>
            {
                new Claim("sub", loginModel.UserName),
                new Claim("name", "Gian Maria"),
                new Claim("role", "Geek")
            };

            var ci = new ClaimsIdentity(claims, "custom-auth-type", "name", "role");
            var cp = new ClaimsPrincipal(ci);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, cp);
            return RedirectToAction("index", "home");
        }
    }
}
