using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using log4net.Core;
using log4net.Layout.Pattern;
using Log4Net.SyslogLayout.Utilities;

namespace Log4Net.SyslogLayout.Converters
{
    /// <summary>
    ///     Provides the currently configured FQDN for the current host. Does not validate name with DNS.
    /// </summary>
    public class HostnameConverter : PatternLayoutConverter
    {
        internal static string GetLocalhostFqdn()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ipProperties.HostName, ipProperties.DomainName);
        }

        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            writer.Write(PrintableAsciiSanitizer.Sanitize(GetLocalhostFqdn().ToUpperInvariant(), 255));
        }
    }
}