﻿using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MixedAspNetAuthentication.Support.Configurations;
using Serilog;

namespace MixedAspNetAuthentication.Support
{
    public static class ServiceConfiguration
    {
        private const string BaseConfigurationFileName = "MixedAspNetAuthentication.json";

        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            var oidcConfiguration = ConfigureSetting<OidcConfiguration>(builder, "oidc");
            var ldapConfiguration = ConfigureSetting<LdapConfiguration>(builder, "ldap");

            builder.Services.AddLogging(cfg => cfg.AddSerilog());

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Authorization/AccessDenied";
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
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
            });
        }

        private static T ConfigureSetting<T>(WebApplicationBuilder builder, string section) where T : class, new()
        {
            builder.Services.Configure<T>(options => builder.Configuration.GetSection(section).Bind(options));
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
                    builder.Configuration.AddJsonFile(overrideFile);
                }
                directoryToCheck = directoryToCheck.Parent;
            }

            Log.Information($"No override configuration file {BaseConfigurationFileName} found starting from directory {AppDomain.CurrentDomain.BaseDirectory}");
        }
    }
}
