using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MixedAspNetAuthentication.Models.Account;
using Novell.Directory.Ldap;
using System.Security.Claims;

namespace MixedAspNetAuthentication.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View(new LoginModel());
        }

        public async Task<IActionResult> PerformLogin(LoginModel loginModel)
        {
            var ldapPort = LdapConnection.DefaultPort;
            int ldapVersion = LdapConnection.LdapV3;
            String ldapHost = "10.0.0.42";
            String loginDN = @$"cyberpunk\{loginModel.UserName}";
            String password1 = loginModel.Password;
            LdapConnection lc = new LdapConnection();

            try
            {
                // connect to the server
                lc.Connect(ldapHost, ldapPort);
                var sdn = lc.GetSchemaDn();

                // authenticate to the server
                lc.Bind(ldapVersion, loginDN, password1);
            }
            catch (LdapException e)
            {
                //Error user is not authenticated.
                loginModel.LoginError = e.ToString();
                return View("Login", loginModel);
            }

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

        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        public IActionResult AzureAADLogin()
        {
            return RedirectToAction("index", "home");
        }
    }
}
