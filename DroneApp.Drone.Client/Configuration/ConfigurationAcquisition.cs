﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AR.Drone.Client.Commands;
using AR.Drone.Data;
using AR.Drone.Data.Navigation;

namespace AR.Drone.Client.Configuration
{
    public class ConfigurationAcquisition
    {
        private const int ControlPort = 5559;
        private const int NetworkBufferSize = 0x10000;
        private const int ConfigTimeout = 1000;

        private readonly DroneClient _client;
        private bool _initialized;

        public ConfigurationAcquisition(DroneClient client)
        {
            _client = client;
        }

        private void OnNavigationData(NavigationData data)
        {
            if (_initialized) return;

            if (data.State.HasFlag(NavigationState.Command))
            {
                _client.Send(new ControlCommand(ControlMode.AckControlMode));
            }
            else
            {
                _client.Send(new ControlCommand(ControlMode.CfgGetControlMode));
                _initialized = true;
            }
        }

        public DroneConfiguration GetConfiguration(CancellationToken token)
        {
            using (var tcpClient = new TcpClient(_client.NetworkConfiguration.DroneHostname, ControlPort))
            using (NetworkStream stream = tcpClient.GetStream())
                try
                {
                    _client.NavigationDataAcquired += OnNavigationData;

                    var buffer = new byte[NetworkBufferSize];
                    Stopwatch swConfigTimeout = Stopwatch.StartNew();
                    while (swConfigTimeout.ElapsedMilliseconds < ConfigTimeout)
                    {
                        token.ThrowIfCancellationRequested();

                        int offset = 0;
                        if (tcpClient.Available == 0)
                        {
                            Thread.Sleep(10);
                        }
                        else
                        {
                            offset += stream.Read(buffer, offset, buffer.Length);
                            swConfigTimeout.Restart();

                            // config eof check
                            if (offset > 0 && buffer[offset - 1] == 0x00)
                            {
                                var data = new byte[offset];
                                Array.Copy(buffer, data, offset);
                                var packet = new ConfigurationPacket
                                    {
                                        Timestamp = DateTime.UtcNow.Ticks,
                                        Data = data
                                    };
                                var configuration = new DroneConfiguration();
                                if (ConfigurationPacketParser.TryUpdate(configuration, packet))
                                {
                                    return configuration;
                                }
                                
                                throw new InvalidDataException();
                            }
                        }
                    }

                    throw new TimeoutException();
                }
                finally
                {
                    _client.NavigationDataAcquired -= OnNavigationData;
                }
        }

        public Task<DroneConfiguration> CreateTask()
        {
            var tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;
            return CreateTask(cancellationToken);
        }

        public Task<DroneConfiguration> CreateTask(CancellationToken cancellationToken)
        {
            return new Task<DroneConfiguration>(() => GetConfiguration(cancellationToken), cancellationToken, TaskCreationOptions.LongRunning);
        }
    }
}