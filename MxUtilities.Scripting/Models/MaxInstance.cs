using MxUtilities.Scripting.Helpers;

namespace MxUtilities.Scripting.Models
{
    public class MaxInstance(IntPtr handle, string title, int processId, string version)
    {
        public IntPtr Handle { get; private set; } = handle;
        public string Title { get; internal set; } = title;
        public int ProcessId { get; private set; } = processId;
        public string Version { get; private set; } = version;

        public void ChangeTitle(string title)
        {
            Title = title;
            _ = WinAPI.SetWindowText(Handle, title);
        }
    }
}
