using System.Diagnostics;
using System.Globalization;
using System.IO;
using log4net.Core;
using log4net.Layout.Pattern;
using Log4Net.SyslogLayout.Utilities;

namespace Log4Net.SyslogLayout.Converters
{
    /// <summary>
    ///     Provides conversion to string the current process ID.
    /// </summary>
    public class ProcessIdConverter : PatternLayoutConverter
    {
        private static string _processId;

        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            _processId = string.IsNullOrEmpty(_processId)
                ? Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture)
                : _processId;

            if (string.IsNullOrEmpty(_processId))
            {
                _processId = "-"; // the NILVALUE
            }

            writer.Write(PrintableAsciiSanitizer.Sanitize(_processId, 48));
        }
    }
}