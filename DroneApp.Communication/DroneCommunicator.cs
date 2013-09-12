using AR.Drone.Client;
using AR.Drone.Client.Commands;
using Debug = System.Diagnostics.Debug;

namespace DroneApp.Communication
{
    public class DroneCommunicator
    {
        private readonly DroneClient _client;
        private readonly CommandReceiver _commandReceiver;

        public DroneCommunicator()
        {
            _client = new DroneClient("192.168.1.248");
            _commandReceiver = new CommandReceiver();
            _commandReceiver.CommandReceived += CommandReceiverOnCommandReceived;
        }

        private void CommandReceiverOnCommandReceived(object sender, CommandEventArgs eventArgs)
        {
            switch (eventArgs.Command)
            {
                case "takeoff":
                    _client.Takeoff();
                    break;
                case "land":
                    _client.Land();
                    break;
                case "hover":
                    _client.Hover();
                    break;
                case "up":
                    _client.Progress(FlightMode.Progressive, gaz: 0.25f);
                    break;
                case "turnleft":
                    _client.Progress(FlightMode.Progressive, yaw: 0.25f);
                    break;
                case "forward":
                    _client.Progress(FlightMode.Progressive, pitch: -0.05f);
                    break;
                case "turnright":
                    _client.Progress(FlightMode.Progressive, yaw: -0.25f);
                    break;
                case "down":
                    _client.Progress(FlightMode.Progressive, gaz: -0.25f);
                    break;
                case "lLeft":
                    _client.Progress(FlightMode.Progressive, yaw: 0.25f);
                    break;
                case "right":
                    _client.Progress(FlightMode.Progressive, roll: 0.05f);
                    break;
                case "back":
                    _client.Progress(FlightMode.Progressive, pitch: 0.05f);
                    break;
                default:
                    Debug.WriteLine("Unknown Command: " + eventArgs.Command);
                    break;
            }
        }

        public void Start()
        {
            _client.Start();
            _commandReceiver.Start();
        }

        public void Stop()
        {
            _commandReceiver.Stop();
            _client.Stop();
        }

        public void TakeOff()
        {
            _client.Takeoff();
        }

        public void Emergency()
        {
            _client.Emergency();
        }

        public void Land()
        {
            _client.Land();
        }
    }
}