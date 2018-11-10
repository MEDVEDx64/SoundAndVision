using System.Net.Sockets;
using Massacre.Snv.Core.Network;
using Massacre.Snv.Core.Stage2;
using Massacre.Snv.Core.Configuration;
using System;
using Massacre.Snv.Core.Network.Packets;
using System.Threading;
using Massacre.Snv.Core.Backend;
using System.Collections.Generic;
using Massacre.Snv.Core.Network.Utils;

namespace Massacre.Snv.Client
{
    class Client : EndPointBase
    {
        ScreenTransmitter transmitter = null;
        Chest chest = Chest.Get();
        int delay = 32;

        public string Name { get; private set; }
        public Guid Id { get; private set; }

        public Client(Socket s) : base(s)
        {
        }

        public void Loop()
        {
            Configure();
            transmitter = new ScreenTransmitter(RemoteIPAddress);

            SendPacket(new LoginPacket()
            {
                Name = Name,
                Id = Id,
                Mode = LoginMode.Client,
                Version = PacketBase.ProtocolVersion
            }).Flush();

            var pkt = new DisplayInfoPacket();
            for(int d = 0; d < transmitter.GetDisplaysCount(); d++)
            {
                var res = SnvBackend.ScrGetResolution(d);
                if(res == 0)
                {
                    goto Halt;
                }

                pkt.Displays.Add(new DisplaySize((ushort)(res >> 16), (ushort)res));
            }

            SendPacket(pkt).Flush();

            while (IsRunning)
            {
                Thread.Sleep(delay);
                if (!transmitter.Send())
                {
                    break;
                }
            }

            Halt:
            transmitter.Close();
            Shutdown();
        }

        void Configure()
        {
            Name = chest.Data.EnsureValue(nameof(Name), "?");

            // ID is no longer required
            //Id = new Guid(chest.Data.EnsureValue(nameof(Id), Guid.NewGuid().ToString()));
            Id = Guid.NewGuid();
        }

        protected override bool HandlePacket(string packet)
        {
            var pingPkt = new PingPacket();
            if(pingPkt.ParsePacket(packet))
            {
                SendPacket(new PongPacket()
                {
                    Text = pingPkt.Text
                }).Flush();
                return true;
            }

            var errPkt = new ErrorPacket();
            if(errPkt.ParsePacket(packet))
            {
                return false;
            }

            var dpPkt = new DisplayPortsPacket();
            if (dpPkt.ParsePacket(packet) && transmitter != null)
            {
                transmitter.Close();
                transmitter.Open(dpPkt.Ports);
                return true;
            }

            var valPkt = new ValuePacket();
            if(valPkt.ParsePacket(packet))
            {
                if(valPkt.Data.ContainsKey("delay"))
                {
                    delay = (int)valPkt.Data["delay"];
                }

                if(valPkt.Data.ContainsKey("den"))
                {
                    var d = new List<uint>();
                    foreach(var v in valPkt.Data["den"].ToString().Split('/'))
                    {
                        d.Add(Convert.ToUInt32(v));
                    }

                    transmitter.SetDenominators(d);
                }

                return true;
            }

            var ntPkt = new NotificationPacket();
            if(ntPkt.ParsePacket(packet))
            {
                SnvBackend.ShowNotification(ntPkt.Text, ntPkt.SoundAlert ? 1 : 0);
                return true;
            }

            return true;
        }
    }
}
