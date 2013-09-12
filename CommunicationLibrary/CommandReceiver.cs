

//using SuperSocket.ClientEngine;
//using WebSocket4Net;
//using WebSocketSharp;
//using DataReceivedEventArgs = WebSocket4Net.DataReceivedEventArgs;

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
//using SuperSocket.ClientEngine;
//using WebSocket4Net;
//using WebSocketSharp;
using WebSocketSharp;
//using DataReceivedEventArgs = WebSocket4Net.DataReceivedEventArgs;

namespace DroneApp
{
    public class CommandReceiver
    {
        private readonly WebSocket _webSocket = new WebSocket("wss://tetherdrone.cloudapp.net/drone/commandsws.ashx");
        public event EventHandler<CommandEventArgs> CommandReceived;
        public event EventHandler WebSocketStateChanged;

        public bool WebsocketReady { get; set; }

        public void Start()
        {
            Thread t = new Thread(Runner);
            t.Name = "Command Receiver";
            t.Start();
        }

        private void Runner()
        {

            while (true)
            {
                GetNextCommand();
                Thread.Sleep(500);
            }

            //_webSocket.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            //_webSocket.OnMessage += WebSocketOnOnMessage;
            //_webSocket.OnClose += WebSocketOnOnClose;
            //_webSocket.OnOpen += WebSocketOnOnOpen;

            //try
            //{
            //    _webSocket.Connect();
            //}
            //catch (Exception)
            //{
            //    Thread.Sleep(100);
            //    Runner();
            //}
            
        }

        private void GetNextCommand()
        {
            var request = WebRequest.Create(new Uri("http://tetherdrone.cloudapp.net/drone/commands.ashx"));
            request.BeginGetResponse(asynchronousResult =>
                {
                    try
                    {
                        WebsocketReady = true;
                        WebSocketStateChanged(this, EventArgs.Empty);
                        var request1 = (HttpWebRequest)asynchronousResult.AsyncState;
                        request1.Timeout = 100;
                        var response = (HttpWebResponse)request1.EndGetResponse(asynchronousResult);
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            string resultString = streamReader.ReadToEnd();
                            if (string.IsNullOrWhiteSpace(resultString.Trim()))
                            {
                                return;
                            }
                            Debug.WriteLine("REsult: " + resultString);
                            string[] lines = resultString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                            foreach (string part in lines)
                            {
                                if (!string.IsNullOrEmpty(part.Trim()))
                                {
                                    CommandReceived(this, new CommandEventArgs { Command = part.Trim() });
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //TODO: Error handling
                    }
                }, request);
        }

        //private void WebSocketOnOnOpen(object sender, EventArgs eventArgs)
        //{
        //    WebsocketReady = true;
        //    WebSocketStateChanged(this, EventArgs.Empty);
        //}

        //private void WebSocketOnOnClose(object sender, CloseEventArgs closeEventArgs)
        //{
        //    WebsocketReady = false;
        //    WebSocketStateChanged(this, EventArgs.Empty);
        //    _webSocket.Connect();
        //}

        //private void WebSocketOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        //{
        //    CommandReceived(this, new CommandEventArgs { Command = messageEventArgs.Data.Trim() });
        //}

        public void Stop()
        {
        }
    }
}

