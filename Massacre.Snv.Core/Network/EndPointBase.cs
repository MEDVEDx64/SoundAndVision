using Massacre.Snv.Core.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Massacre.Snv.Core.Network
{
    public abstract class EndPointBase
    {
        public static readonly ushort Port = 8664;

        Socket socket = null;
        bool running = false;
        bool shutdown = false;
        Action callback = null;
        Queue<string> sendQueue = new Queue<string>();

        Thread clThread = null;

        public event EventHandler<EndPointExceptionEventArgs> Faulted;
        public event EventHandler<EventArgs> Disconnected;

        public bool IsRunning
        {
            get { return running; }
        }

        public IPAddress RemoteIPAddress
        {
            get { return ((IPEndPoint)socket.RemoteEndPoint).Address; }
        }

        public EndPointBase(Socket s)
        {
            socket = s;
            clThread = new Thread(ClientThread);
        }

        public void SetShutdownCallback(Action callback)
        {
            this.callback = callback;
        }

        public void Start()
        {
            if (!running)
            {
                running = true;
                clThread.Start();
            }
        }

        void ClientThread()
        {
            try
            {
                socket.Blocking = false;
                var check = new List<Socket>();

                var buffer = new byte[0x1000];
                var current = new StringBuilder();

                while (!shutdown)
                {
                    check.Add(socket);
                    Socket.Select(check, null, null, 1000000);
                    if(check.Count == 0)
                    {
                        continue;
                    }

                    var len = socket.Receive(buffer, buffer.Length, SocketFlags.None);
                    if (len == 0)
                    {
                        break;
                    }

                    foreach (var c in PacketBase.Encoding.GetString(buffer, 0, len))
                    {
                        if (c == PacketBase.Delimiter)
                        {
                            var result = false;
                            AppTools.TryInvoke(() =>
                            {
                                result = HandlePacket(current.ToString());
                            });

                            if (!result)
                            {
                                Flush();
                                goto Breakout;
                            }

                            current.Clear();
                            continue;
                        }

                        current.Append(c);
                    }

                    Flush();
                }

                Breakout:;
            }

            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                AppTools.TryInvoke(() =>
                {
                    Faulted?.Invoke(this, new EndPointExceptionEventArgs(e));
                });
            }

            finally
            {
                try
                {
                    socket.Close();
                    socket = null;
                    if (callback != null)
                    {
                        AppTools.TryInvoke(() =>
                        {
                            callback();
                        });
                    }
                }
                catch (Exception ea)
                {
                    AppTools.TryInvoke(() =>
                    {
                        Faulted?.Invoke(this, new EndPointExceptionEventArgs(ea));
                    });
                }

                shutdown = false;
                running = false;
                AppTools.TryInvoke(() =>
                {
                    Disconnected?.Invoke(this, new EventArgs());
                });
            }
        }

        protected virtual bool HandlePacket(string packet)
        {
            throw new NotImplementedException();
        }

        public EndPointBase SendPacket(PacketBase pkt)
        {
            sendQueue.Enqueue(pkt.ToString());
            return this;
        }

        public EndPointBase Flush()
        {
            StringBuilder pack = new StringBuilder();
            while (sendQueue.Count > 0)
            {
                pack.Append(sendQueue.Dequeue());
            }

            if (socket != null && pack.Length > 0)
            {
                var bytes = PacketBase.Encoding.GetBytes(pack.ToString());
                socket.Send(bytes, bytes.Length, SocketFlags.None);
            }

            return this;
        }

        public void Shutdown(Exception e = null)
        {
            if(running)
            {
                if(e != null)
                {
                    AppTools.TryInvoke(() =>
                    {
                        Faulted?.Invoke(this, new EndPointExceptionEventArgs(e));
                    });
                }

                shutdown = true;
            }
        }
    }
}
