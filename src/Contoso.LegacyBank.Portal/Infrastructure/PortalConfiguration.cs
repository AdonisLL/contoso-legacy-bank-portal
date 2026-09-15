using System;
using System.Configuration;

namespace Contoso.LegacyBank.Portal.Infrastructure
{
    public sealed class PortalConfiguration
    {
        public Uri AccountServiceUrl { get; private set; }
        public Uri StatementApiUrl { get; private set; }

        public static PortalConfiguration Load()
        {
            return new PortalConfiguration
            {
                AccountServiceUrl = ReadUri("AccountServiceUrl", "http://localhost:8090/AccountService"),
                StatementApiUrl = ReadUri("StatementApiUrl", "http://localhost:8091/api/statements")
            };
        }

        private static Uri ReadUri(string key, string fallback)
        {
            var value = ConfigurationManager.AppSettings[key];
            Uri uri;
            if (!Uri.TryCreate(string.IsNullOrWhiteSpace(value) ? fallback : value, UriKind.Absolute, out uri))
            {
                throw new ConfigurationErrorsException("The " + key + " application setting must be an absolute URL.");
            }

            return uri;
        }
    }
}
