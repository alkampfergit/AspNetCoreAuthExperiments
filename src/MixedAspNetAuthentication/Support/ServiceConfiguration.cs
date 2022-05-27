using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MixedAspNetAuthentication.Support.Configurations;
using Serilog;

namespace MixedAspNetAuthentication.Support
{
    public static class ServiceConfiguration
    {
        private const string BaseConfigurationFileName = "MixedAspNetAuthentication.json";

        private static TimeSpan CookieValidityTimespan = TimeSpan.FromDays(10);

        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            var oidcConfiguration = ConfigureSetting<OidcConfiguration>(builder, "oidc");
            var ldapConfiguration = ConfigureSetting<LdapConfiguration>(builder, "ldap");

            builder.ConfigureSecurityProvider();

            //builder.Services.AddHttpContextAccessor();
            //builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Authorization/AccessDenied";
                options.ExpireTimeSpan = CookieValidityTimespan;

                options.Cookie.MaxAge = CookieValidityTimespan;
                options.Cookie.Name = "AuthCookie";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                
                //This will made Azure AAD oidc process to go in loop
                //options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.IsEssential = true;

                // sliding expiration
                //options.SlidingExpiration = true;
                //options.EventsType = typeof(CustomCookieAuthenticationEvents);
                //options.DataProtectionProvider = CookieDataProtectionProvider;
            })
            .ConfigureOidc(oidcConfiguration)
            .ConfigureNegotiate();
        }

        private static T ConfigureSetting<T>(WebApplicationBuilder builder, string section) where T : class, new()
        {
            builder.Services.Configure<T>(builder.Configuration.GetSection(section));
            var configuration = new T();
            builder.Configuration.Bind(section, configuration);
            return configuration;
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
                    builder.Configuration.AddJsonFile(overrideFile, optional: false, reloadOnChange: true);
                }
                directoryToCheck = directoryToCheck.Parent;
            }

            Log.Information($"No override configuration file {BaseConfigurationFileName} found starting from directory {AppDomain.CurrentDomain.BaseDirectory}");
        }
    }
}
