using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AR.Drone.Client;
using AR.Drone.Client.Commands;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Widget;
using CommunicationLibrary;
using Java.Net;
using Debug = System.Diagnostics.Debug;

namespace DroneApp
{
    [Activity(Label = "DroneApp", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity, ILocationListener
    {
        private DroneClient _client;
        private LocationManager _locationManager;
        private TextView _logTextView;
        private TextView _dronStateTextView;
        private TextView _websocketTextView;
        private CheckBox _checkBox;
        private CommandReceiver _receiver;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            InitializeLocationManager();

            SetContentView(Resource.Layout.Main);

            FindViewById<Button>(Resource.Id.MyButtonLand).Click += LandButtonClick;
            FindViewById<Button>(Resource.Id.ButtonEmergency).Click += EmergencyButtonClicked;
            FindViewById<Button>(Resource.Id.buttonReset).Click += (sender, args) => _client.ResetEmergency();
            _checkBox = FindViewById<CheckBox>(Resource.Id.executeCommandsCheckBox);
            _checkBox.CheckedChange += CheckBoxOnCheckedChange;

            _logTextView = FindViewById<TextView>(Resource.Id.logTextView);
            _dronStateTextView = FindViewById<TextView>(Resource.Id.droneStateTextView);
            _websocketTextView = FindViewById<TextView>(Resource.Id.serviceStateTextView);

            _client = new DroneClient("192.168.1.248");
            _client.Start();

            //_client.NavigationDataAcquired += ClientOnNavigationDataAcquired;

            _receiver = new CommandReceiver();
            _receiver.CommandReceived += ReceiverOnCommandReceived;
            _receiver.WebSocketStateChanged += ReceiverOnWebSocketStateChanged;
            _receiver.Start();

            var droneConnectionChecker = new Thread(ConnectionCheckRunner) {Name = "Drone Checker"};
            droneConnectionChecker.Start();
        }

        private void CheckBoxOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs checkedChangeEventArgs)
        {
            _executeCommands = _checkBox.Checked;
        }

        private void ReceiverOnWebSocketStateChanged(object sender, EventArgs eventArgs)
        {
            if (_receiver.WebsocketReady)
            {
                RunOnUiThread(() => _websocketTextView.SetBackgroundColor(new Color(0, 255, 0)));
            }
            else
            {
                RunOnUiThread(() => _websocketTextView.SetBackgroundColor(new Color(255, 0, 0)));
                _client.Hover();
            }
        }

        private void ConnectionCheckRunner()
        {
            while (true)
            {
                try
                {
                    var addr = InetAddress.GetByName("192.168.1.248");

                    if (addr.IsReachable(50))
                    {
                        RunOnUiThread(() => _dronStateTextView.SetBackgroundColor(new Color(0, 255, 0)));
                    }
                    else
                    {
                        RunOnUiThread(() => _dronStateTextView.SetBackgroundColor(new Color(255, 0, 0)));
                    }

                    Thread.Sleep(100);

                }
                catch (UnknownHostException)
                {
                    RunOnUiThread(() => _dronStateTextView.SetBackgroundColor(new Color(255, 0, 0)));
                }
                catch (IOException)
                {
                    RunOnUiThread(() => _dronStateTextView.SetBackgroundColor(new Color(255, 0, 0)));
                }
            }

        }

        protected override void OnResume()
        {
            base.OnResume();
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(new Criteria(), true);
            foreach (var acceptableLocationProvider in acceptableLocationProviders)
            {
                _locationManager.RequestLocationUpdates(acceptableLocationProvider, 1000, 0, this);
            }
            
        }

        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
        }

        private void InitializeLocationManager()
        {
            _locationManager = (LocationManager)GetSystemService(LocationService);
        }

        private void EmergencyButtonClicked(object sender, EventArgs eventArgs)
        {
            Log("Local Command: Emergency");
            _client.Emergency();

        }

        private void ReceiverOnCommandReceived(object sender, CommandEventArgs eventArgs)
        {
            Log(eventArgs.Command);
            if (!_executeCommands)
            {
                return;
            }
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
                    _client.Progress(FlightMode.Progressive, pitch: -0.5f);
                    break;
                case "turnright":
                    _client.Progress(FlightMode.Progressive, yaw: -0.25f);
                    break;
                case "down":
                    _client.Progress(FlightMode.Progressive, gaz: -0.25f);
                    break;
                case "left":
                    _client.Progress(FlightMode.Progressive, yaw: 0.25f);
                    break;
                case "right":
                    _client.Progress(FlightMode.Progressive, roll: 0.05f);
                    break;
                case "back":
                    _client.Progress(FlightMode.Progressive, pitch: 0.5f);
                    break;
                default:
                    Debug.WriteLine("Unknown Command: " + eventArgs.Command);
                    break;
            }
        }

        private bool _executeCommands;

        //private void ClientOnNavigationDataAcquired(NavigationData navigationData)
        //{
        //    //count++;
        //    //if (count % 100 == 0)
        //    //{
        //    //    Log("DUmp" + count + " / " + (navigationData.Magneto.Rectified.Y) + "/" + navigationData.Magneto.Heading);
        //    //}

        //    //Debug.WriteLine("Dump");
        //    //Debug.WriteLine("Data aquired: " + navigationData.Battery.Percentage);
        //    //Log(navigationData.Magneto.Radius + "");
        //    //Sender.SendNavigationData(navigationData);
        //}

        private void LandButtonClick(object sender, EventArgs eventArgs)
        {
            Log("Local Command: Land");
            _client.Land();
        }

        public void OnLocationChanged(Location location)
        {
            if (location != null)
            {
                Log("Location: " + location.Longitude + "/" + location.Latitude + "/" + location.Accuracy + "m");
                Sender.SendGpsLocation(
                    new
                        {
                            location.Longitude,
                            location.Latitude,
                            location.Bearing,
                            location.Speed,
                            location.Accuracy,
                            location.Altitude,
                            location.Time
                        });
            }

        }

        private void Log(string data)
        {
            RunOnUiThread(() =>
                {
                    Debug.WriteLine(data);
                    _logTextView.Text = data + "\r\n" + _logTextView.Text;
                });
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        }
    }


}

