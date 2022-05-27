using Microsoft.AspNetCore.DataProtection;
using Serilog;

namespace MixedAspNetAuthentication.Support
{
    internal static class SecurityConfiguration
    {
        internal static void ConfigureSecurityProvider(this WebApplicationBuilder builder)
        {
            var keyDirectoryService = builder.Configuration.GetValue<string>("DataProtectionDirectory");
            if (String.IsNullOrEmpty(keyDirectoryService))
            {
                keyDirectoryService = Path.Combine(Path.GetTempPath(), "MixedAspNetAuthenticationKeys");
            }
            if (!Directory.Exists(keyDirectoryService))
            {
                Directory.CreateDirectory(keyDirectoryService);
            }
            builder.Services.AddLogging(cfg => cfg.AddSerilog());

            //Configure a dataprovider, it will be useful to decrypt AuthCookie 
            var dataProtectionBuilder = builder.Services.AddDataProtection()
               .PersistKeysToFileSystem(new DirectoryInfo(keyDirectoryService))
               .ProtectKeysWithDpapiNG()
               .SetApplicationName("MixedAspNetAuthentication");
        }
    }
}
