using MxUtilities.Scripting.Helpers;
using MxUtilities.Scripting.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MxUtilities.Scripting
{
    internal class MaxInstanceMonitor
    {
        private readonly List<MaxInstance> _instances;
        private readonly CancellationTokenSource _cts;
        private readonly int _delay;

        public event Action<MaxInstance>? InstanceStarted;
        public event Action<MaxInstance>? InstanceExited;
        public event Action<MaxInstance, string>? InstanceTitleChanged;

        internal MaxInstanceMonitor(List<MaxInstance> instances, int delay = 1000)
        {
            _instances = instances;
            _cts = new CancellationTokenSource();
            _delay = delay;
        }

        public void StartMonitoring()
        {
            Task.Run(async () =>
            {
                var previousTitles = _instances.ToDictionary(instance => instance.Handle, instance => instance.Title);

                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        foreach (var maxInstance in _instances.ToList())
                        {
                            var process = Process.GetProcesses().FirstOrDefault(p => p.Id == maxInstance.ProcessId);
                            if (process == null || process.HasExited)
                            {
                                InstanceExited?.Invoke(maxInstance);
                                _instances.Remove(maxInstance);
                                previousTitles.Remove(maxInstance.Handle);
                                continue;
                            }

                            var currentTitle = WinAPI.GetWindowTitle(maxInstance.Handle);
                            if (currentTitle != null && previousTitles[maxInstance.Handle] != currentTitle)
                            {
                                maxInstance.Title = currentTitle;
                                previousTitles[maxInstance.Handle] = currentTitle;
                                InstanceTitleChanged?.Invoke(maxInstance, currentTitle);
                            }
                        }

                        var processes = Process.GetProcessesByName("3dsmax");
                        foreach (var process in processes)
                        {
                            if (process.MainWindowHandle != IntPtr.Zero && !_instances.Any(instance => instance.Handle == process.MainWindowHandle))
                            {
                                var currentTitle = process.MainWindowTitle;
                                var match = InstanceManager.WindowTitleRegex.Match(currentTitle);

                                if (match.Success)
                                {
                                    var version = match.Groups[2].Value;
                                    var instance = new MaxInstance(process.MainWindowHandle, currentTitle, process.Id, version);

                                    _instances.Add(instance);
                                    previousTitles[instance.Handle] = instance.Title;
                                    InstanceStarted?.Invoke(instance);
                                }
                            }
                        }

                        await Task.Delay(_delay, _cts.Token);
                    }
                    catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
                    {
                        break;
                    }
                }
            }, _cts.Token);
        }

        public void StopMonitoring() => _cts.Cancel();
    }
}
