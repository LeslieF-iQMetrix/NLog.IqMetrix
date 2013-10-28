using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using NLog.Core.Targets;

namespace NLog.Android
{
    [Target("LumberMill")]
    public sealed class LumberMillTarget : TargetWithLayout
    {
        public LumberMillTarget(string androidId)
        {
            ServerUrl = new Uri("http://10.200.25.18:8008/log");
            AndroidId = "and-" + androidId;
            HttpClient = new HttpClient();
        }

        private Uri ServerUrl { get; set; }
        private HttpClient HttpClient { get; set; }
        private HttpWebResponse LastResponseMessage { get; set; }
        private string AndroidId { get; set; }

        protected override async void Write(Core.LogEventInfo logEvent)
        {

            var request = (HttpWebRequest) WebRequest.Create(ServerUrl);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "POST";
            var requestWriter = new StreamWriter(request.GetRequestStream());
            
            requestWriter.Write("{ "
                                + "\"version\": " + "\"" + "1.0" + "\",\n"
                                + "\"host\": " + "\"" + AndroidId + "\",\n"
                                + "\"short_message\": " + "\"" + logEvent.FormattedMessage + "\",\n"
                                + "\"full_message\": " + "\"" + logEvent.FormattedMessage + "\",\n"
                                + "\"timestamp\": " + "\"" + DateTime.Now.ToString(CultureInfo.InvariantCulture) +
                                "\",\n"
                                + "\"level\": " + "\"" +
                                logEvent.Level.Ordinal.ToString(CultureInfo.InvariantCulture) + "\",\n"
                                + "\"facility\": " + "\"" + "NLog Android Test" + "\",\n"
                                + "\"file\": " + "\"" + Environment.CurrentDirectory + "AndroidApp" + "\",\n"
                                + "\"line\": " + "\"" + "123" + "\",\n"
                                + "\"Userdefinedfields\": " + "{}" + "\n"
                                + "}");

            requestWriter.Close();

            LastResponseMessage = (HttpWebResponse) request.GetResponse();
        }
    }
}