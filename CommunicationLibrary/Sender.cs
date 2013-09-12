using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace CommunicationLibrary
{

    public class Sender
    {
        public static void SendGpsLocation(object location)
        {
            SendData(location, "http://tetherdrone.cloudapp.net/drone/gps.ashx");
        }

        public static void SendNavigationData(object data)
        {
            SendData(data, "http://tetherdrone.cloudapp.net/drone/navigation.ashx");
        }

        private static void SendData(object data, string url)
        {
            string json = JsonConvert.SerializeObject(data);

            var request = (HttpWebRequest)WebRequest.Create(url);
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
