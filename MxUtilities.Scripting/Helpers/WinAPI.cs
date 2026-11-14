using System.Runtime.InteropServices;
using System.Text;

namespace MxUtilities.Scripting.Helpers
{
    internal static class WinAPI
    {
        public const uint WM_CHAR = 0x0102;
        public const uint WM_SETTEXT = 0x000C;
        public const IntPtr VK_RETURN = 0x0D;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string? className, string? windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string? lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int SetWindowText(IntPtr hWnd, string? text);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public static string? GetWindowTitle(IntPtr hWnd)
        {
            const int maxChars = 256;
            StringBuilder titleBuffer = new(maxChars);

            if (GetWindowText(hWnd, titleBuffer, maxChars) > 0)
            {
                return titleBuffer.ToString();
            }

            return null;
        }
    }
}
