using Massacre.Snv.Core.Backend;
using Massacre.Snv.Core.Backend.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Massacre.Snv.Core.Video
{
    public class ScreenRecorder
    {
        bool running = true;
        Thread recorderThread = null;

        internal static bool IsApplicationRunning { get; set; } = true;

        public ScreenRecorder()
        {
            recorderThread = new Thread(RecorderThread);
            recorderThread.Start();
        }

        void RecorderThread()
        {
            Dictionary<int, IntPtr> contexts = new Dictionary<int, IntPtr>();
            while (running && IsApplicationRunning)
            {
                unsafe
                {
                    try
                    {
                        var displays = SnvBackend.ScrCountDisplays();
                        if (displays < 0)
                        {
                            throw new Exception("Cannot count the displays available");
                        }

                        for (int i = 0; i < displays; i++)
                        {
                            var ptr = Marshal.AllocHGlobal(sizeof(RecorderContext));
                            UnmanagedTools.SetMemory(ptr, 0, sizeof(RecorderContext));
                            contexts.Add(i, ptr);
                        }

                        while (running && IsApplicationRunning)
                        {
                            foreach(var k in contexts.Keys)
                            {
                                S2Frame frmRaw;
                                if (SnvBackend.ScrCapture(&frmRaw, k, 1) != 0)
                                {
                                    break;
                                }

                                SnvBackend.RecCommitFrame((RecorderContext*)contexts[k], &frmRaw, k);
                                SnvBackend.FreeFrame(&frmRaw);
                            }

                            Thread.Sleep(25);
                        }
                    }

                    catch { }

                    try
                    {
                        SnvBackend.RecStop();
                        foreach (var k in contexts.Keys)
                        {
                            SnvBackend.RecCommitFrame((RecorderContext*)contexts[k], null, k);
                            Marshal.FreeHGlobal(contexts[k]);
                        }

                        contexts.Clear();
                    }

                    catch { }
                }

                Thread.Sleep(250);
            }
        }

        public void Shutdown()
        {
            running = false;
            recorderThread.Join();
        }
    }
}
