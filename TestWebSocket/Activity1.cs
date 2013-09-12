using System;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WebSocketSharp;
using Debug = System.Diagnostics.Debug;

namespace TestWebSocket
{
    [Activity(Label = "TestWebSocket", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        int count = 1;
        private readonly WebSocket _webSocket = new WebSocket("wss://echo.websocket.org");


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            FindViewById<Button>(Resource.Id.MyButton1).Click += OnClick;

            button.Click += ButtonOnClick;
        }

        private void OnClick(object sender, EventArgs eventArgs)
        {
            _webSocket.Send("Message" + count);
            count++;
        }

        private void ButtonOnClick(object sender, EventArgs eventArgs)
        {

            //_webSocket.Closed += WebSocketOnClosed;
            //_webSocket.DataReceived += WebSocketOnDataReceived;
            //_webSocket.MessageReceived += WebSocketOnMessageReceived;
            //_webSocket.Error += WebSocketOnError;

            //_webSocket.Open();
            //while (!(_webSocket.State == WebSocketState.Open))
            //{
            //    Thread.Sleep(10);
            //}

            //_webSocket.Send("Hallo welt");

            _webSocket.OnMessage += WebSocketOnOnMessage;
            _webSocket.Connect();
            while (_webSocket.ReadyState != WebSocketState.OPEN)
            {
                Thread.Sleep(10);
            } 
            
            _webSocket.Send("Test");
        }

        private void WebSocketOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            Debug.WriteLine("Message: " + messageEventArgs.Data);
        }


        //private void WebSocketOnMessageReceived(object sender, MessageReceivedEventArgs messageReceivedEventArgs)
        //{
        //    Console.WriteLine(messageReceivedEventArgs.Message);
        //}

        //private void WebSocketOnClosed(object sender, EventArgs eventArgs)
        //{
        //    Debug.WriteLine("WebSocket closed -> ReOpen");
        //    _webSocket.Open();
        //}

        //private void WebSocketOnError(object sender, ErrorEventArgs errorEventArgs)
        //{
        //    Debug.WriteLine("WebSocket Error: " + errorEventArgs.Exception);
        //}

        //private void WebSocketOnDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        //{
        //    string command = Encoding.ASCII.GetString(dataReceivedEventArgs.Data);
        //    Console.WriteLine(command);
        //    Debug.WriteLine("NEW DATA: " + command);
        //}
    }
}

