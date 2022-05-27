using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MixedAspNetAuthentication.Support.Configurations;
using System.Security.Claims;

namespace MixedAspNetAuthentication.Support
{
    internal static class OpenIdConfiguration
    {
        internal static AuthenticationBuilder ConfigureOidc(this AuthenticationBuilder builder, OidcConfiguration oidcConfiguration)
        {
            builder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = oidcConfiguration.Authority;
                options.ClientId = oidcConfiguration.ClientId;
                options.ClientSecret = oidcConfiguration.ClientSecret;
                options.RequireHttpsMetadata = true;
                options.ResponseType = "code";

                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.GivenName,
                    RoleClaimType = JwtClaimTypes.Role
                };

                options.Events.OnTokenValidated = OnTokenValidated;
            });

            return builder;
        }

        internal static Task OnTokenValidated(TokenValidatedContext ctx)
        {
            string sub = ctx.Principal!.Claims.Single(c => c.Type == "aud").Value;

            //Get context usually to access database or service that contains your user information
            //var db = ctx.HttpContext.RequestServices.GetRequiredService<AuthorizationDbContext>();

            string internalUserId = "User_23";

            //Add every claims you need
            var claims = new List<Claim>
            {
                new Claim("internalId", internalUserId)
            };
            var appIdentity = new ClaimsIdentity(claims);

            ctx.Principal.AddIdentity(appIdentity);
            return Task.CompletedTask;
        }
    }
}
