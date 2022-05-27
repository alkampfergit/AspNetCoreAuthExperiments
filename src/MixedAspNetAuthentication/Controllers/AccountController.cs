using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MixedAspNetAuthentication.Models.Account;
using MixedAspNetAuthentication.Support.Configurations;
using Novell.Directory.Ldap;
using System.Security.Claims;

namespace MixedAspNetAuthentication.Controllers
{
    public class AccountController : Controller
    {
        private readonly IOptionsMonitor<LdapConfiguration> _ldapConfiguration;

        public AccountController(
            IOptionsMonitor<LdapConfiguration> ldapConfiguration)
        {
            _ldapConfiguration = ldapConfiguration;
        }

        public IActionResult Login()
        {
            return View(new LoginModel());
        }

        public async Task< IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> PerformADLogin(LoginModel loginModel)
        {
            var ldapPort = LdapConnection.DefaultPort;
            int ldapVersion = LdapConnection.LdapV3;
            String ldapHost = _ldapConfiguration.CurrentValue.Address;
            String loginDN = @$"{_ldapConfiguration.CurrentValue.DomainName}\{loginModel.UserName}";
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

            //now you should check the user and grab all the groups if needed.
            var claims = new List<Claim>
            {
                new Claim("sub", loginModel.UserName),
                new Claim("name", "Gian Maria"),
                new Claim("role", "Geek")
            };

            var ci = new ClaimsIdentity(claims, "ad", "name", "role");
            var cp = new ClaimsPrincipal(ci);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, cp);
            return RedirectToAction("index", "home");
        }

        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        public IActionResult AzureAADLogin()
        {
            return RedirectToAction("index", "home");
        }

        [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
        public async Task<IActionResult> NegotiateLogin()
        {
            //Get claims principal that was created by the negotiate authentication scheme
            var user = HttpContext.User;

            //Now we will create a standard claim as for login with username and password.
            var claims = new List<Claim>
            {
                new Claim("sub", user.Identity!.Name!),
                new Claim("name", "Gian Maria"),
                new Claim("role", "Geek")
            };

            var ci = new ClaimsIdentity(claims, "ad", "name", "role");
            var cp = new ClaimsPrincipal(ci);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, cp);
            return RedirectToAction("index", "home");
        }
    }
}
