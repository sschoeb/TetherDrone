using System;
using System.IO;
using System.Net;
using System.Threading;

namespace DroneApp.Communication
{
    public class CommandReceiver
    {
        private Thread _thread;
        private bool _running;

        public event EventHandler<CommandEventArgs> CommandReceived;

        public void Start()
        {
            _running = true;
            _thread = new Thread(Runner);
        }

        private void Runner()
        {
            while (_running)
            {
                WebCommunicator.ReadCommandData(command => CommandReceived(this, new CommandEventArgs { Command = command }));
            }
        }

        public void Stop()
        {
            _running = false;
            _thread.Join();
            _thread = null;
        }

    }
}