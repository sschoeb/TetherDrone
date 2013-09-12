using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ClientEngine;
using WebSocket4Net;
using DataReceivedEventArgs = WebSocket4Net.DataReceivedEventArgs;

namespace WebSocketConsole
{
    class Program
    {
        private readonly WebSocket _webSocket = new WebSocket("ws://tetherdrone.cloudapp.net/drone/commandsws.ashx");
        

        static void Main(string[] args)
        {
            new Program();
            Console.ReadLine();
        }

        public Program()
        {

            _webSocket.Closed += WebSocketOnClosed;
            _webSocket.DataReceived += WebSocketOnDataReceived;
            _webSocket.MessageReceived += WebSocketOnMessageReceived;
            _webSocket.Error += WebSocketOnError;

            _webSocket.Open();
        }

        private void WebSocketOnMessageReceived(object sender, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            Console.WriteLine(messageReceivedEventArgs.Message);
        }

        private void WebSocketOnClosed(object sender, EventArgs eventArgs)
        {
                Debug.WriteLine("WebSocket closed -> ReOpen");
                _webSocket.Open();
        }

        private void WebSocketOnError(object sender, ErrorEventArgs errorEventArgs)
        {
            Debug.WriteLine("WebSocket Error: " + errorEventArgs.Exception);
        }

        private void WebSocketOnDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            string command = Encoding.ASCII.GetString(dataReceivedEventArgs.Data);
            Console.WriteLine(command);
            Debug.WriteLine("NEW DATA: " + command);
        }
    }
}
