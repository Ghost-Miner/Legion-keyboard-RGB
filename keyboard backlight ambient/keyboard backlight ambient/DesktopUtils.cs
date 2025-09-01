using System.Runtime.InteropServices;


public static class DesktopUtils
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr OpenInputDesktop(uint dwFlags, bool inherit, uint desiredAccess);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool CloseDesktop(IntPtr hDesktop);

    // Force the W (Unicode) version explicitly
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "GetUserObjectInformationW")]
    static extern bool GetUserObjectInformation(
        IntPtr hObj,
        int nIndex,
        IntPtr pvInfo,
        int nLength,               // BYTES
        out int lpnLength);        // BYTES

    const int UOI_NAME = 2;
    const uint DESKTOP_READOBJECTS = 0x0001;

    public static string GetCurrentDesktopName()
    {
        IntPtr hDesk = OpenInputDesktop(0, false, DESKTOP_READOBJECTS);
        if (hDesk == IntPtr.Zero) return null;

        try
        {
            // 1) Probe size (should fail with ERROR_INSUFFICIENT_BUFFER and set lpnLength)
            int needed;
            GetUserObjectInformation(hDesk, UOI_NAME, IntPtr.Zero, 0, out needed);
            if (needed <= 0) return null;

            // 2) Allocate that many BYTES
            IntPtr buf = Marshal.AllocHGlobal(needed);
            try
            {
                if (!GetUserObjectInformation(hDesk, UOI_NAME, buf, needed, out needed))
                {
                    return null;
                }

                // 3) Decode as UTF-16LE; needed is BYTES, so divide by 2 for chars
                string name = Marshal.PtrToStringUni(buf, needed / 2);

                // Trim at NUL if present (defensive)
                int nul = name.IndexOf('\0');
                if (nul >= 0) name = name[..nul];

                return name;
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }
        finally
        {
            CloseDesktop(hDesk);
        }
    }

    public static bool IsInteractiveDesktop()
    => GetCurrentDesktopName()?.Equals("Default", StringComparison.OrdinalIgnoreCase) == true;

}