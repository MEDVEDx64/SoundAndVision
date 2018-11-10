using Massacre.Snv.Core.Backend;
using Massacre.Snv.Core.Backend.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace Massacre.Snv.Core.Stage2
{
    public class ScreenTransmitter
    {
        IPAddress serverAddr = null;
        IDictionary<int, IntPtr> contexts = new Dictionary<int, IntPtr>();
        IEnumerable<int> ctxKeys = null;
        List<uint> denominators = null;

        public event EventHandler<EventArgs> Closed;
        public static readonly uint InitialDenominator = 8;

        public ScreenTransmitter(IPAddress serverAddr)
        {
            this.serverAddr = serverAddr;
        }

        public int GetDisplaysCount()
        {
            var displays = SnvBackend.ScrCountDisplays();
            if(displays < 0)
            {
                throw new Exception("Cannot count displays available");
            }

            return displays;
        }

        public void Open(IReadOnlyList<ushort> ports)
        {
            if (contexts.Count > 0)
            {
                throw new InvalidOperationException("Bug: please close the transmitter before opening it again");
            }

            denominators = new List<uint>();
            for(int d = 0; d < ports.Count; d++)
            {
                unsafe
                {
                    var res = SnvBackend.ScrGetResolution(d);
                    if (res == 0)
                    {
                        throw new Exception("Cannot determine display resolution");
                    }

                    S2TransmitterContext* tc = (S2TransmitterContext*)Marshal.AllocHGlobal(sizeof(S2TransmitterContext));
                    if (SnvBackend.S2TransmitterInit(tc, serverAddr.ToString(), ports[d], 0) != 0)
                    {
                        throw new InvalidOperationException("Failed to initialize video stream transmission");
                    }

                    contexts.Add(d, (IntPtr)tc);
                }

                denominators.Add(InitialDenominator);
            }

            ctxKeys = contexts.Keys.ToArray();
        }

        public void SetDenominators(IEnumerable<uint> d)
        {
            if(contexts.Count == 0)
            {
                return;
            }

            if (d.Count() < ctxKeys.Count())
            {
                denominators = new List<uint>();
                for (int i = 0; i < ctxKeys.Count(); i++)
                {
                    denominators.Add(1);
                }

                int x = 0;
                foreach(var den in d)
                {
                    denominators[x] = den;
                    x++;
                }
            }

            else
            {
                denominators = d.ToList();
            }
        }

        public bool Send()
        {
            if(ctxKeys == null)
            {
                return true;
            }

            int kn = 0;
            foreach(var k in ctxKeys)
            {
                unsafe
                {
                    S2Frame frm;

                    if(SnvBackend.ScrCapture(&frm, k, denominators[kn]) != 0)
                    {
                        return false;
                    }

                    var tc = contexts[k];
                    var err = SnvBackend.S2Send((S2TransmitterContext*)tc, &frm);
                    contexts[k] = tc;
                    SnvBackend.FreeFrame(&frm);

                    if (err != 0)
                    {
                        return false;
                    }
                }

                kn++;
            }

            return true;
        }

        public void Close()
        {
            foreach (var k in contexts.Keys)
            {
                unsafe
                {
                    var tc = contexts[k];
                    SnvBackend.S2TransmitterClose((S2TransmitterContext*)tc);
                    Marshal.FreeHGlobal(tc);
                }
            }

            contexts.Clear();
            ctxKeys = null;
            denominators = null;
            Closed?.Invoke(this, new EventArgs());
        }
    }
}
