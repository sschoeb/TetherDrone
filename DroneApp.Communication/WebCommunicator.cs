using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace DroneApp.Communication
{

    public class WebCommunicator
    {
        private const string BaseUrl = "http://tetherdrone.cloudapp.net/drone/{0}.ashx";

        public static void SendGpsLocation(object location)
        {
            SendData(location, "gps");
        }

        public static void SendNavigationData(object data)
        {
            SendData(data, "navigation");
        }

        public static void ReadCommandData(Action<string> commandReceived)
        {
            var request = WebRequest.Create(FormatUrl("command"));
            request.Timeout = 50000;
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    commandReceived(line);
                }
            }
        }

        private static string FormatUrl(string parameter)
        {
            return string.Format(BaseUrl, parameter);
        }

        private static void SendData(object data, string urlParameter)
        {
            string json = JsonConvert.SerializeObject(data);

            var request = (HttpWebRequest)WebRequest.Create(FormatUrl(urlParameter));
            request.Method = "POST";
            request.ContentType = "application/json";

            request.BeginGetRequestStream(result =>
                {
                    var requestInternal = (HttpWebRequest)result.AsyncState;

                    using (Stream stream = requestInternal.EndGetRequestStream(result))
                    {
                        stream.Write(Encoding.UTF8.GetBytes(json), 0, Encoding.UTF8.GetBytes(json).Length);
                    }

                    requestInternal.BeginGetResponse(GetResponseCallback, request);
                }, request);
        }

        private static void GetResponseCallback(IAsyncResult result)
        {
            var request = (HttpWebRequest)result.AsyncState;
            try
            {
                var response = (HttpWebResponse)request.EndGetResponse(result);
                Debug.WriteLine("Response: " + response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fail to write:" + ex.Message);
            }

        }
    }
}
