

//using SuperSocket.ClientEngine;
//using WebSocket4Net;
//using WebSocketSharp;
//using DataReceivedEventArgs = WebSocket4Net.DataReceivedEventArgs;

using System;
using System.Diagnostics;
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
            Debug.WriteLine("Start command listener");
            _webSocket.Log.Level = LogLevel.DEBUG;
            _webSocket.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            _webSocket.OnMessage += WebSocketOnOnMessage;
            _webSocket.OnClose += WebSocketOnOnClose;
            _webSocket.OnOpen += WebSocketOnOnOpen;
            _webSocket.Connect();
        }

        private void WebSocketOnOnOpen(object sender, EventArgs eventArgs)
        {
            WebsocketReady = true;
            WebSocketStateChanged(this, EventArgs.Empty);
            
        }

        private void WebSocketOnOnClose(object sender, CloseEventArgs closeEventArgs)
        {
            WebsocketReady = false;
            WebSocketStateChanged(this, EventArgs.Empty);
            _webSocket.Connect();
        }

        private void WebSocketOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            CommandReceived(this, new CommandEventArgs { Command = messageEventArgs.Data.Trim() });
        }


        public void Stop()
        {
        }

    }
}

