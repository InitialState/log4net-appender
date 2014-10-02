using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using log4net.Appender;
using log4net.Core;
using log4net.Util;
using Newtonsoft.Json;

namespace Log4Net.InitialStateAppender
{
    public class ApiAppender : AppenderSkeleton
    {
        public string ApiKey { get; set; }
        public string ApiRootUrl { get; set; }
        public Guid BucketId { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                var thread = new Thread(ShipLogMessage);
                thread.IsBackground = true;
                thread.Start(loggingEvent);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void ShipLogMessage(object loggingEvent)
        {
            try
            {
                var typedLoggingEvent = (LoggingEvent) loggingEvent;
                string logMessage = RenderLoggingEvent(typedLoggingEvent);

                Trace.WriteLine(logMessage);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiRootUrl);

                    string trackerId = null;
                    var ndc = typedLoggingEvent.LookupProperty("NDC") as ThreadContextStack;
                    if (ndc != null && ndc.Count > 0)
                    {
                        // the NDC represents a context stack, whose levels are separated by whitespace. we will use this as our MessageId.
                        trackerId = ndc.ToString();
                    }
                    var logMessageRequest = new LogMessageRequest
                                            {
                                                Log = logMessage,
                                                DateTime = DateTime.UtcNow,
                                                SignalSource =
                                                    typedLoggingEvent
                                                    .LoggerName,
                                                TrackerId = trackerId
                                            };
                    client.PostAsJsonAsync(string.Format("{0}/{1}", BucketId, ApiKey), logMessageRequest);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private class LogMessageRequest
        {
            [JsonProperty(PropertyName = "log")]
            public string Log { get; set; }

            [JsonProperty(PropertyName = "date_time")]
            public DateTime DateTime { get; set; }

            [JsonProperty(PropertyName = "signal_source")]
            public string SignalSource { get; set; }

            [JsonProperty(PropertyName = "tracker_id")]
            public string TrackerId { get; set; }
        }
    }
}