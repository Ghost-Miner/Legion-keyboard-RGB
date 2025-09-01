using System.Runtime.InteropServices;

internal class SessionWatcher : Form
{
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        // Register for session notifications
        NativeMethods.WTSRegisterSessionNotification(this.Handle,
            NativeMethods.NOTIFY_FOR_THIS_SESSION);
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_WTSSESSION_CHANGE = 0x02B1;
        base.WndProc(ref m);

        if (m.Msg == WM_WTSSESSION_CHANGE)
        {
            var changeType = (int)m.WParam;
            Console.WriteLine($"Session event: {changeType}");
            // WTS_SESSION_LOCK, WTS_SESSION_UNLOCK, etc.
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        NativeMethods.WTSUnRegisterSessionNotification(this.Handle);
        base.OnHandleDestroyed(e);
    }

    private static class NativeMethods
    {
        public const int NOTIFY_FOR_THIS_SESSION = 0;
        [DllImport("Wtsapi32.dll")]
        public static extern bool WTSRegisterSessionNotification(IntPtr hWnd, int dwFlags);
        [DllImport("Wtsapi32.dll")]
        public static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);
    }
}