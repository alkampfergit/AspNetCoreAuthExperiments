using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MixedAspNetAuthentication.Support;
using Serilog;
using System.Text;

namespace MixedAspNetAuthentication.Controllers
{
    [Authorize]
    [Route("api/diagnostic")]
    [ApiController]
    public class DiagnosticController : ControllerBase
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public DiagnosticController(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtectionProvider = dataProtectionProvider;
        }

        [HttpGet]
        [Route("test")]
        public IActionResult Test() 
        {
            var protector = _dataProtectionProvider.CreateProtector("myprotector");
            var encrypted = protector.Protect(Encoding.UTF8.GetBytes("ciao mondo"));

            var decrypted = Encoding.UTF8.GetString(protector.Unprotect(encrypted));

            var wrongProtector = _dataProtectionProvider.CreateProtector("wrongprotector");
            decrypted = Encoding.UTF8.GetString(wrongProtector.Unprotect(encrypted));
            return Ok();
        }

        [HttpGet]
        [Route("dump-cookie")]
        public IActionResult Get()
        {
            StringBuilder authCookieValue = new StringBuilder(100);
            var baseCookie = Request.Cookies["AuthCookie"]!;
            if (baseCookie.StartsWith("chunks-"))
            {
                Log.Debug("We have chunked cookie: {chunks}", baseCookie);
                var orderedCookieChunks = Request
                    .Cookies
                    .Where(c => c.Key.StartsWith("AuthCookieC"))
                    .OrderBy(c => c.Key);

                foreach (var cookieChunk in orderedCookieChunks)
                {
                    authCookieValue.Append(cookieChunk.Value);
                }
            }
            else
            {
                authCookieValue.Append(baseCookie);
            }
            List<string> cookieNames = new List<string>();
            foreach (var cookie in Request.Cookies)
            {
                cookieNames.Add(cookie.Key);
            }

            var rawCookieValue = authCookieValue.ToString();

            // ONE - grab the CookieAuthenticationOptions instance
            var opt = HttpContext.RequestServices
                .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(CookieAuthenticationDefaults.AuthenticationScheme); //or use .Get("Cookies")

            // TWO - Get the encrypted cookie value
            var requestCookie = opt.CookieManager.GetRequestCookie(HttpContext, opt.Cookie.Name);

            var customReadok = requestCookie.Equals(authCookieValue.ToString());

            // THREE - decrypt it
            var decrypted = opt.TicketDataFormat.Unprotect(requestCookie);

            var decoded = Base64UrlTextEncoder.Decode(rawCookieValue);
            var protector = _dataProtectionProvider.CreateProtector("AuthCookie");
            //var realCookieContent = protector.Unprotect(decoded);

            return Ok(new
            {
                Cookies = cookieNames,
                //Principal = decrypted.Principal,
                //Properties = decrypted.Properties,
                CustomReadok = customReadok,
                RawCookieValue = authCookieValue.ToString()
            });
        }
    }
}
