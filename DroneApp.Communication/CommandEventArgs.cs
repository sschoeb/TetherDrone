using System;

namespace DroneApp.Communication
{
    public class CommandEventArgs : EventArgs
    {
        public string Command { get; set; }
    }
}