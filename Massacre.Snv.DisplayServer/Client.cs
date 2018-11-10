using Massacre.Snv.Core.Network;
using Massacre.Snv.Core.Network.Packets;
using Massacre.Snv.Core.Utils;
using Massacre.Snv.DisplayServer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Massacre.Snv.DisplayServer
{
    public class Client : EndPointBase, INotifyPropertyChanged
    {
        readonly static int PingTimeout = 30000; // milliseconds
        static readonly int startingPort = 4554;
        static readonly int portMax = 65500;
        bool IsAnyPongsAccepted = true;

        Server server = null;
        string name = null;
        string logName = null;
        LoginMode mode;
        Guid? id;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DisplaysChangedEventArgs> DisplaysChanged;
        public event EventHandler<ValueCarrierEventArgs<string>> NameChanged;
        public event EventHandler<ValueCarrierEventArgs<Guid?>> IdChanged;

        public string Name
        {
            get { return name ?? "(?)"; }
        }

        public Guid? Id
        {
            get { return id; }
        }

        public ObservableCollection<Display> Displays { get; }

        public Client(Socket s, Server srv) : base(s)
        {
            server = srv;
            logName = s.LocalEndPoint.ToString();

            server.PrintLog(logName + " connected");

            Displays = new ObservableCollection<Display>();
            Displays.CollectionChanged += OnDisplaysCollectionChanged;
            Disconnected += OnClientDisconnected;
            Faulted += OnClientFaulted;

            // Pinging thread
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(PingTimeout);
                    if (!IsRunning)
                    {
                        break;
                    }

                    if (!IsAnyPongsAccepted)
                    {
                        Shutdown();
                        break;
                    }

                    IsAnyPongsAccepted = false;
                    SendPacket(new PingPacket()).Flush();
                }
            })
            {
                IsBackground = true
            }.Start();
        }

        public IEnumerable<uint> GetDenominators()
        {
            List<uint> den = new List<uint>();
            foreach(var d in Displays)
            {
                den.Add(d.SuggestedDenominator);
            }

            return den;
        }

        private void OnDisplaysCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Display d in e.OldItems)
                {
                    d.Interrupt();
                }
            }

            if (e.NewItems != null)
            {
                foreach (Display d in e.NewItems)
                {
                    d.Run();
                }
            }

            DisplaysChanged?.Invoke(this, new DisplaysChangedEventArgs(this, e.OldItems, e.NewItems));
        }

        private void OnClientDisconnected(object sender, EventArgs e)
        {
            server.PrintLog(logName + " disconnected");
            PurgeDisplays();
        }

        private void OnClientFaulted(object sender, EndPointExceptionEventArgs e)
        {
            server.PrintLog(logName + ": <" + e.ExceptionObject.GetType() + "> " + e.ExceptionObject.Message);
        }

        protected override bool HandlePacket(string packet)
        {
            var pongPkt = new PongPacket();
            if (pongPkt.ParsePacket(packet))
            {
                IsAnyPongsAccepted = true;
                return true;
            }

            var loginPkt = new LoginPacket();
            if (loginPkt.ParsePacket(packet))
            {
                if (loginPkt.Version != PacketBase.ProtocolVersion)
                {
                    SendPacket(new ErrorPacket()
                    {
                        Text = "Unsupported protocol"
                    });
                    return false;
                }

                if (loginPkt.Id == null && Id == null)
                {
                    SendPacket(new ErrorPacket()
                    {
                        Text = "Please provide me with ID"
                    });
                    return true;
                }

                if (loginPkt.Id != null)
                {
                    foreach (var cl in server.Clients)
                    {
                        if(cl == this || cl.Id == null)
                        {
                            continue;
                        }

                        /* ID check disabled
                        if (cl.Id == loginPkt.Id)
                        {
                            SendPacket(new ErrorPacket()
                            {
                                Text = "ID is already registered"
                            });
                            return true;
                        }
                        */
                    }

                    id = (Guid)loginPkt.Id;
                    IdChanged?.Invoke(this, new ValueCarrierEventArgs<Guid?>(id));
                }

                name = loginPkt.Name;
                mode = loginPkt.Mode;
                SendPacket(new AckPacket());

                var ln = (name == null ? "" : name + " ") + "(" + (id == null ? "?" : id.ToString().Substring(0, 8)) + ")";
                server.PrintLog(logName + " has registered as " + ln);
                logName = ln;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                NameChanged?.Invoke(this, new ValueCarrierEventArgs<string>(name));
                return true;
            }

            if(mode == LoginMode.Unspecified)
            {
                SendPacket(new ErrorPacket()
                {
                    Text = "Not logged in"
                });
                return true;
            }

            var displayPkt = new DisplayInfoPacket();
            if (displayPkt.ParsePacket(packet))
            {
                PurgeDisplays();
                var port = startingPort - 1;
                var ports = new List<ushort>();
                for(int d = 0; d < displayPkt.Displays.Count; d++)
                {
                    port++;
                    if(port > portMax)
                    {
                        throw new Exception("No more network ports available in range");
                    }

                    if(!IsTcpPortAvailable((ushort)port))
                    {
                        d--;
                        continue;
                    }

                    var ds = displayPkt.Displays[d];
                    Displays.Add(new Display((ushort)port, this, ds.Width, ds.Height));
                    ports.Add((ushort)port);
                }

                SendPacket(new DisplayPortsPacket(ports)).Flush();
                return true;
            }

            var pingPkt = new PingPacket();
            if (pingPkt.ParsePacket(packet))
            {
                SendPacket(new PongPacket()
                {
                    Text = pingPkt.Text
                });
                return true;
            }

            return true;
        }

        bool IsTcpPortAvailable(ushort port)
        {
            return !((from p in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
                    where p.Port == port select p).Count() == 1);
        }

        void PurgeDisplays()
        {
            foreach (var d in Displays.ToArray())
            {
                Displays.Remove(d);
            }
        }
    }
}
