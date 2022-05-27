using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Serilog;
using System.Security.Claims;

namespace MixedAspNetAuthentication.Support
{
    internal static class NegotiateConfiguration
    {
        internal static AuthenticationBuilder ConfigureNegotiate(this AuthenticationBuilder builder)
        {
            var provider = builder.Services.BuildServiceProvider();
            builder.AddNegotiate();
            return builder;
        }
    }
}
