using MxUtilities.Scripting.Helpers;
using MxUtilities.Scripting.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MxUtilities.Scripting
{
    public partial class InstanceManager
    {
        internal static readonly Regex WindowTitleRegex = CreateWindowTitleRegex();
        private readonly Dictionary<IntPtr, IntPtr> _scintillaHandles;
        private readonly List<MaxInstance> _maxInstances;
        private readonly MaxInstanceMonitor _monitor;

        public event Action<MaxInstance>? InstanceStarted;
        public event Action<MaxInstance>? InstanceExited;
        public event Action<MaxInstance, string>? InstanceTitleChanged;

        public InstanceManager()
        {
            _scintillaHandles = [];
            _maxInstances = [];
            _monitor = new MaxInstanceMonitor(_maxInstances);

            _monitor.InstanceStarted += OnInstanceStarted;
            _monitor.InstanceExited += OnInstanceExited;
            _monitor.InstanceTitleChanged += OnInstanceTitleChanged;
        }

        public void StartMonitoring()
        {
            _monitor.StartMonitoring();
        }

        public void StopMonitoring()
        {
            _monitor.StopMonitoring();
        }

        private void OnInstanceStarted(MaxInstance instance) => InstanceStarted?.Invoke(instance);

        private void OnInstanceExited(MaxInstance instance) => InstanceExited?.Invoke(instance);

        private void OnInstanceTitleChanged(MaxInstance instance, string newTitle) => InstanceTitleChanged?.Invoke(instance, newTitle);

        public static IReadOnlyList<MaxInstance> GetActiveInstances()
        {
            var activeInstances = new List<MaxInstance>();

            var processes = Process.GetProcessesByName("3dsmax");
            foreach (var process in processes)
            {
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    var currentTitle = process.MainWindowTitle;
                    var match = WindowTitleRegex.Match(currentTitle);

                    if (match.Success)
                    {
                        var version = match.Groups[2].Value;
                        var instance = new MaxInstance(process.MainWindowHandle, currentTitle, process.Id, version);
                        activeInstances.Add(instance);
                    }
                }
            }

            return activeInstances.AsReadOnly();
        }

        public bool SendMessage(MaxInstance maxInstance, string message)
        {
            message += "\r\n";

            if (!_scintillaHandles.TryGetValue(maxInstance.Handle, out IntPtr scintillaHandle))
            {
                scintillaHandle = FindScintillaHandleInChildren(maxInstance.Handle);
                if (scintillaHandle == IntPtr.Zero) return false;
                _scintillaHandles[maxInstance.Handle] = scintillaHandle;
            }

            WinAPI.SendMessage(scintillaHandle, WinAPI.WM_SETTEXT, IntPtr.Zero, message);
            WinAPI.SendMessage(scintillaHandle, WinAPI.WM_CHAR, WinAPI.VK_RETURN, IntPtr.Zero);

            return true;
        }

        private static IntPtr FindScintillaHandleInChildren(IntPtr window)
        {
            IntPtr statusPanelHandle = WinAPI.FindWindowEx(window, IntPtr.Zero, "StatusPanel", null);
            if (statusPanelHandle != IntPtr.Zero)
            {
                IntPtr statusPanelScintilla = FindHandleRecursively(statusPanelHandle, "MXS_Scintilla");
                if (statusPanelScintilla != IntPtr.Zero)
                {
                    return statusPanelScintilla;
                }
            }

            return FindHandleRecursively(window, "MXS_Scintilla");
        }

        private static IntPtr FindHandleRecursively(IntPtr window, string targetClassName)
        {
            IntPtr childHandle = IntPtr.Zero;
            while ((childHandle = WinAPI.FindWindowEx(window, childHandle, null, null)) != IntPtr.Zero)
            {
                IntPtr foundHandle = WinAPI.FindWindowEx(childHandle, IntPtr.Zero, targetClassName, null);
                if (foundHandle != IntPtr.Zero)
                    return foundHandle;

                IntPtr descendantHandle = FindHandleRecursively(childHandle, targetClassName);
                if (descendantHandle != IntPtr.Zero)
                    return descendantHandle;
            }

            return IntPtr.Zero;
        }

        [GeneratedRegex(@"^(.* - Autodesk 3ds Max (\d{4})\s*)$", RegexOptions.Compiled)]
        internal static partial Regex CreateWindowTitleRegex();
    }
}
