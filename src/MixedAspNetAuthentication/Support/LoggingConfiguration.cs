using Serilog;

namespace MixedAspNetAuthentication.Support
{
    public static class LoggingConfiguration
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            var log = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.WithProperty("service", "Proxy")
                .CreateLogger();

            Log.Logger = log;
        }
    }
}
