using Massacre.Snv.Core.Backend.Primitives;
using System.Runtime.InteropServices;

namespace Massacre.Snv.Core.Backend
{
    public static class SnvBackend
    {
        const string DLL32 = "snvbackend";

        [DllImport(DLL32, EntryPoint = "snv_init", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Init();

        [DllImport(DLL32, EntryPoint = "snv_quit", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Quit();

        [DllImport(DLL32, EntryPoint = "s2_alloc_frame", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void AllocFrame(S2Frame* frm, ushort w, ushort h);

        [DllImport(DLL32, EntryPoint = "s2_free_frame", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void FreeFrame(S2Frame* frm);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int ReceiverCallback(S2Frame* frm);

        [DllImport(DLL32, EntryPoint = "s2_recv_thread", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int ReceiverThread(S2ReceiverContext* rctx, ushort port, int flags, ReceiverCallback callback);

        [DllImport(DLL32, EntryPoint = "s2_send_init", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int S2TransmitterInit(S2TransmitterContext* tctx,
            [MarshalAs(UnmanagedType.LPStr)] string addr, ushort port, int flags);

        [DllImport(DLL32, EntryPoint = "s2_send", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int S2Send(S2TransmitterContext* tctx, S2Frame* frm);

        [DllImport(DLL32, EntryPoint = "s2_send_close", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int S2TransmitterClose(S2TransmitterContext* tctx);

        [DllImport(DLL32, EntryPoint = "scr_count_displays", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ScrCountDisplays();

        [DllImport(DLL32, EntryPoint = "scr_get_resolution", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ScrGetResolution(int display);

        [DllImport(DLL32, EntryPoint = "scr_capture", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern int ScrCapture(S2Frame *frm, int display, uint den);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void ClientIconCallback();

        [DllImport(DLL32, EntryPoint = "snv_setup_client_icon", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetupClientIcon(ClientIconCallback callback);

        [DllImport(DLL32, EntryPoint = "snv_show_notification", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ShowNotification([MarshalAs(UnmanagedType.LPWStr)] string text, int beep);

        [DllImport(DLL32, EntryPoint = "snv_remove_client_icon", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RemoveClientIcon();

        [DllImport(DLL32, EntryPoint = "rec_is_running", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RecIsRunning();

        [DllImport(DLL32, EntryPoint = "rec_get_sessid", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RecGetSessionId();

        [DllImport(DLL32, EntryPoint = "rec_get_settings_window_flag", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RecGetSettingsWindowFlag();

        [DllImport(DLL32, EntryPoint = "rec_set_settings_window_flag", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RecSetSettingsWindowFlag(int value);

        [DllImport(DLL32, EntryPoint = "rec_start", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RecStart();

        [DllImport(DLL32, EntryPoint = "rec_stop", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RecStop();

        [DllImport(DLL32, EntryPoint = "rec_commit_frame", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void RecCommitFrame(RecorderContext *ctx, S2Frame *frm, int display);
    }
}
