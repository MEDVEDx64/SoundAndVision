using Massacre.Snv.Core.Backend;
using Massacre.Snv.Core.Backend.Primitives;
using Massacre.Snv.Core.Utils;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Massacre.Snv.Core.Stage2
{
    public class S2Receiver
    {
        ushort recvPort = 0;

        Thread rcThread = null;
        IntPtr rcPtr = IntPtr.Zero;
        WriteableBitmap bitmap = null;
        SnvBackend.ReceiverCallback callback = null;

        public event EventHandler<FrameAcceptedEventArgs> FrameAccepted;
        public event EventHandler<ReceiverInterruptedEventArgs> Interrupted;

        public S2Receiver(ushort port)
        {
            recvPort = port;
            unsafe
            {
                callback += FrameAcceptedCallback;
            }
        }

        public void Run()
        {
            if(rcThread != null)
            {
                throw new InvalidOperationException("Thread cannot be run twice");
            }

            rcThread = new Thread(() =>
            {
                unsafe
                {
                    int err = 0;
                    Exception eObj = null;

                    try
                    {
                        rcPtr = Marshal.AllocHGlobal(sizeof(S2ReceiverContext));
                        err = SnvBackend.ReceiverThread((S2ReceiverContext*)rcPtr, recvPort, 0, callback);
                    }

                    catch (ThreadAbortException) { }
                    catch (Exception e)
                    {
                        eObj = e;
                    }

                    finally
                    {
                        AppTools.TryInvoke(() =>
                        {
                            Interrupted?.Invoke(this, new ReceiverInterruptedEventArgs(err, eObj));
                        });

                        Marshal.FreeHGlobal(rcPtr);
                        rcPtr = IntPtr.Zero;
                        rcThread = null;
                    }
                }
            });

            rcThread.IsBackground = true;
            rcThread.Start();
        }

        public void Interrupt()
        {
            unsafe
            {
                if (rcPtr != IntPtr.Zero)
                {
                    ((S2ReceiverContext*)rcPtr)->Terminate = 1;
                }
            }

            // Forcing interruption of non-responding thread
            new Thread(() =>
            {
                Thread.Sleep(1500);
                if(rcThread != null)
                {
                    rcThread.Abort();
                }
            })
            {
                IsBackground = true
            }.Start();
        }

        unsafe int FrameAcceptedCallback(S2Frame *frm)
        {
            if(frm->Error != 0 || frm->Width == 0 || frm->Height == 0)
            {
                return 0;
            }

            if(FrameAccepted == null)
            {
                SnvBackend.FreeFrame(frm);
                return 0;
            }

            AppTools.TryInvoke(() =>
            {
                bool changed = false;
                if(bitmap == null || bitmap.Width != frm->Width || bitmap.Height != frm->Height)
                {
                    bitmap = new WriteableBitmap(frm->Width, frm->Height, 96, 96, PixelFormats.Bgr24, null);
                    changed = true;
                }

                unsafe
                {
                    bitmap.Lock();
                    UnmanagedTools.CopyMemory(bitmap.BackBuffer, frm->Pixels, (uint)(frm->Height * bitmap.BackBufferStride));
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, frm->Width, frm->Height));
                    bitmap.Unlock();

                    SnvBackend.FreeFrame(frm);
                }

                FrameAccepted?.Invoke(this, new FrameAcceptedEventArgs(bitmap, changed));
            });

            return 0;
        }
    }
}
