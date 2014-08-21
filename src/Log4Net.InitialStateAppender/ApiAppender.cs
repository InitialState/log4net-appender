using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using log4net.Appender;
using log4net.Core;
using Newtonsoft.Json;

namespace Log4Net.InitialStateAppender
{
    public class ApiAppender : AppenderSkeleton
    {
        private const string TrackerEndToken = "</tid>";
        private const string TrackerBeginToken = "<tid>";
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
                string url = string.Format("{0}{1}?clientKey={2}", ApiRootUrl, BucketId, ApiKey);

                Trace.WriteLine(url);
                Trace.WriteLine(logMessage);

                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Method = "POST";

                //speed up the request by not autodetecting proxy http://go.microsoft.com/fwlink/?linkid=14202
                webRequest.Proxy = null;
                webRequest.ContentType = "application/json";

                string trackerId = null;
                var trackerStartIndex = 0;
                var trackerLength = 0;
                if (logMessage.Contains(TrackerBeginToken))
                {
                    try
                    {
                        trackerStartIndex = logMessage.IndexOf(TrackerBeginToken) + TrackerBeginToken.Length;
                        trackerLength = logMessage.IndexOf(TrackerEndToken) - trackerStartIndex;
                        trackerId = logMessage.Substring(trackerStartIndex, trackerLength);
                    }
                    catch (Exception ex)
                    {// we want to ignore exceptions here because the most critical part is logging the line.
                        trackerId = string.Format("tid_ex({1},{2}): {0}", ex.Message, trackerStartIndex, trackerLength);
                        Trace.WriteLine(ex.Message);
                    }
                }
                
                string json = JsonConvert.SerializeObject(new LogMessageRequest
                                                          {
                                                              Log = logMessage,
                                                              DateTime = DateTime.UtcNow,
                                                              SignalSource = typedLoggingEvent.LoggerName,
                                                              TrackerId = trackerId
                                                          });
                byte[] contentBytes = Encoding.UTF8.GetBytes(json);
                webRequest.ContentLength = contentBytes.Length;
                Stream stream = webRequest.GetRequestStream();
                stream.Write(contentBytes, 0, contentBytes.Length);
                stream.Flush();
                stream.Close();

                using (webRequest.GetResponse())
                {
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private class LogMessageRequest
        {
            public string Log { get; set; }
            public DateTime DateTime { get; set; }
            public string SignalSource { get; set; }
            public string TrackerId { get; set; }
        }
    }
}