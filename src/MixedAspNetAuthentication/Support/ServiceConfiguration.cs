using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace MixedAspNetAuthentication.Support
{
    public static class ServiceConfiguration
    {
        private const string BaseConfigurationFileName = "MixedAspNetAuthentication.json";

        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddLogging(cfg => cfg.AddSerilog());

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.AccessDeniedPath = "/Authorization/AccessDenied";
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = "https://login.microsoftonline.com/854d7cc7-3d8e-42db-b62b-39ff06ca250f/";
                options.ClientId = "06947bf7-75e5-4e3f-aa87-162b3681d50f";
                options.ClientSecret = "cc28Q~~oQ2eZST3YcZBFxo9EumH5C3w82hJrqaM0";
                options.RequireHttpsMetadata = true;
                options.ResponseType = "code";

                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.GivenName,
                    RoleClaimType = JwtClaimTypes.Role
                };
            });
        }

        internal static void AddAdditionalConfigurationSources(WebApplicationBuilder builder)
        {
            var directoryToCheck = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent;
            Log.Information($"Starting to look override file {BaseConfigurationFileName} starting from directory {AppDomain.CurrentDomain.BaseDirectory}");
            while (directoryToCheck != null)
            {
                string overrideFile = Path.Combine(directoryToCheck.FullName, BaseConfigurationFileName);
                if (File.Exists(overrideFile))
                {
                    Log.Information("Found override configuration file: {overrideConfigFile}", overrideFile);
                    builder.Configuration.AddJsonFile(overrideFile);
                }
                directoryToCheck = directoryToCheck.Parent;
            }

            Log.Information($"No override configuration file {BaseConfigurationFileName} found starting from directory {AppDomain.CurrentDomain.BaseDirectory}");
        }
    }
}
