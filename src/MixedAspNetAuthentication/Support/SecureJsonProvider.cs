using Microsoft.Extensions.Configuration.Json;

namespace MixedAspNetAuthentication.Support
{
    public static class SecureJsonConfigurationExtensions
    {
        public static IConfigurationBuilder AddSecureJson(this IConfigurationBuilder builder, String jsonFilePath, bool reloadOnChange)
        {
            var source = new SecureJsonConfigurationSource(jsonFilePath, reloadOnChange);
            builder.Add(source);
            return builder;
        }
    }

    public class SecureJsonConfigurationSource : IConfigurationSource
    {
        private readonly string _jsonFilePath;
        private readonly bool _reloadOnChange;

        public SecureJsonConfigurationSource(String jsonFilePath, bool reloadOnChange)
        {
            _jsonFilePath = jsonFilePath;
            _reloadOnChange = reloadOnChange;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            JsonConfigurationSource source = new JsonConfigurationSource();
            source.Path = _jsonFilePath;
            source.ReloadOnChange = _reloadOnChange;
            source.ResolveFileProvider();
            return new SecureJsonProvider(source);
        }
    }

    public class SecureJsonProvider : JsonConfigurationProvider
    {
        public SecureJsonProvider(JsonConfigurationSource source) : base(source)
        {
        }

        public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            return base.GetChildKeys(earlierKeys, parentPath);
        }

        public override void Set(string key, string value)
        {
            base.Set(key, value);
        }
    }
}
