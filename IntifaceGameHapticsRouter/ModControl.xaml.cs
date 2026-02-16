using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using EasyHook;
using NLog;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace IntifaceGameHapticsRouter
{
    /// <summary>
    /// Interaction logic for ProcessControl.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>

    public partial class ModControl
    {
        public EventHandler<GHRProtocolMessageContainer> MessageReceivedHandler;
        
        private static string GetProcessUser(Process process)
        {
            IntPtr processHandle = IntPtr.Zero;
            try
            {
                OpenProcessToken(process.Handle, 8, out processHandle);
                WindowsIdentity wi = new WindowsIdentity(processHandle);
                string user = wi.Name;
                return user.Contains(@"\") ? user.Substring(user.IndexOf(@"\") + 1) : user;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                }
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        public class ProcessInfo
        {
            public string FileName;
            public int Id;
            public string Owner;

            public override string ToString()
            {
                var f = System.IO.Path.GetFileNameWithoutExtension(FileName);
                return $"{f} ({Id})";
            }

            public bool IsLive => Process.GetProcessById(Id) != null;
        }

        private class ProcessInfoList : ObservableCollection<ProcessInfo>
        {
        }

        public bool Attached
        {
            get
            {
                return _attached;
            }
            set
            {
                _attached = value;
                ProcessListBox.IsEnabled = !value;
                SearchBox.IsEnabled = !value;
                RefreshButton.IsEnabled = !value;
                AttachButton.IsEnabled = true;
                AttachButton.Content = value ? "Detach From Process" : "Attach To Process";
            }
        }

        public string ProcessStatus
        {
            set { StatusLabel.Content = value; }
        }

        private static readonly string[] _systemOwners = { "SYSTEM", "LOCAL SERVICE", "NETWORK SERVICE" };
        private static readonly string _windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        private static bool IsSystemProcess(Process proc, string owner)
        {
            if (_systemOwners.Any(s => owner.EndsWith(s, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            try
            {
                var fileName = proc.MainModule?.FileName;
                if (!string.IsNullOrEmpty(fileName) &&
                    fileName.StartsWith(_windowsDir, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch
            {
                // Cannot access MainModule for protected/elevated processes
            }

            return false;
        }

        private ProcessInfoList _processList = new ProcessInfoList();
        private List<ProcessInfo> _allProcesses = new List<ProcessInfo>();

        public event EventHandler<EventArgs> ProcessAttached;
        public event EventHandler<EventArgs> ProcessDetached;

        private bool _attached = false;
        private readonly Logger _log;
        private Task _enumProcessTask;
//        private UnityVRMod _unityMod;
        private EasyHookMod _easyHookMod;
        private CancellationTokenSource _scanningTokenSource = null;
        private CancellationToken _scanningToken;

        private const string SearchPlaceholder = "Click to search...";

        public ModControl()
        {
            InitializeComponent();
            _log = LogManager.GetCurrentClassLogger();
            ProcessListBox.ItemsSource = _processList;
            ProcessListBox.SelectionChanged += OnSelectionChanged;
            SearchBox.Text = SearchPlaceholder;

            RunEnumProcessUpdate();
        }

        private void RunEnumProcessUpdate()
        {
            if (_scanningTokenSource != null)
            {
                _scanningTokenSource.Cancel();
                _enumProcessTask.Wait();
            }
            _scanningTokenSource = new CancellationTokenSource();
            _scanningToken = _scanningTokenSource.Token;
            _enumProcessTask = new Task(() => EnumProcesses());
            _enumProcessTask.Start();
        }

        private void EnumProcesses()
        {
            Dispatcher.Invoke(() => { _processList.Clear(); });
            Dispatcher.Invoke(() => { ProcessStatus = "Scanning Processes..."; });
            var cp = Process.GetCurrentProcess().Id;
            var procList = Process.GetProcesses();
            var results = new ConcurrentBag<ProcessInfo>();

            Parallel.ForEach(procList, (currentProc) =>
            {
                if (_scanningToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    // This can sometimes happen between calling GetProcesses and getting here. Save ourselves the throw.
                    if (currentProc.HasExited || currentProc.Id == cp)
                    {
                        return;
                    }

                    // Only check process identity - no module scanning
                    var owner = RemoteHooking.GetProcessIdentity(currentProc.Id).Name;

                    if (!string.IsNullOrEmpty(owner) && !IsSystemProcess(currentProc, owner))
                    {
                        results.Add(new ProcessInfo
                        {
                            FileName = currentProc.ProcessName,
                            Id = currentProc.Id,
                            Owner = owner,
                        });
                    }
                }
                catch (AccessViolationException)
                {
                    // noop, there's a lot of system processes we can't see.
                }
                catch (Win32Exception)
                {
                    // noop, there's a lot of system processes we can't see.
                }
                catch (Exception aEx)
                {
                    _log.Error(aEx);
                }
            });

            var sorted = results.OrderBy(p => p.FileName, StringComparer.OrdinalIgnoreCase).ToList();

            Dispatcher.Invoke(() =>
            {
                _allProcesses = sorted;
                FilterProcessList();
            });

            if (!_attached)
            {
                Dispatcher.Invoke(() => { ProcessStatus = "Select Process to Inject"; });
            }
            _scanningTokenSource = null;
            _enumProcessTask = null;
        }

        private static bool FuzzyMatch(string text, string query)
        {
            int qi = 0;
            for (int ti = 0; ti < text.Length && qi < query.Length; ti++)
            {
                if (char.ToLowerInvariant(text[ti]) == char.ToLowerInvariant(query[qi]))
                {
                    qi++;
                }
            }
            return qi == query.Length;
        }

        private void FilterProcessList()
        {
            var raw = SearchBox.Text.Trim();
            var query = (raw == SearchPlaceholder) ? "" : raw;
            _processList.Clear();

            foreach (var proc in _allProcesses)
            {
                if (string.IsNullOrEmpty(query) || FuzzyMatch(proc.FileName, query))
                {
                    _processList.Add(proc);
                }
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (SearchBox.Foreground != System.Windows.Media.Brushes.Gray)
            {
                FilterProcessList();
            }
        }

        private void SearchBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (SearchBox.Text == SearchPlaceholder)
            {
                SearchBox.Text = "";
                SearchBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = SearchPlaceholder;
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void OnSelectionChanged(object aObj, EventArgs aEvent)
        {
            AttachButton.IsEnabled = ProcessListBox.SelectedItems.Count == 1;
        }

        private void OnMessageReceived(object aObj, GHRProtocolMessageContainer aMsg)
        {
          MessageReceivedHandler?.Invoke(this, aMsg);
        }

        private void AttachButton_Click(object aObj, System.Windows.RoutedEventArgs aEvent)
        {
            if (!Attached)
            {
                if (_scanningTokenSource != null && _scanningToken.CanBeCanceled)
                {
                    _scanningTokenSource.Cancel();
                }

                var process = ProcessListBox.SelectedItems.Cast<ProcessInfo>().ToList()[0];
                if (!process.IsLive)
                {
                    return;
                }

                AttachButton.IsEnabled = false;
                RefreshButton.IsEnabled = false;
                ProcessListBox.IsEnabled = false;

                var attached = false;

                // Try XInput first (most common), then fall back to UWP
                try
                {
                    _easyHookMod = new XInputMod();
                    _easyHookMod.Attach(process.Id);
                    _easyHookMod.MessageReceivedHandler += OnMessageReceived;
                    attached = true;
                }
                catch (Exception ex)
                {
                    _log.Warn($"XInput injection failed for {process.FileName}: {ex.Message}");
                    _easyHookMod = null;

                    // Fall back to UWP
                    try
                    {
                        _easyHookMod = new UWPInputMod();
                        _easyHookMod.Attach(process.Id);
                        _easyHookMod.MessageReceivedHandler += OnMessageReceived;
                        attached = true;
                    }
                    catch (Exception uwpEx)
                    {
                        _log.Warn($"UWP injection also failed for {process.FileName}: {uwpEx.Message}");
                        _easyHookMod = null;
                    }
                }

                if (attached)
                {
                    Attached = true;
                    ProcessAttached?.Invoke(this, null);
                    ProcessStatus = $"Attached to {process.FileName} ({process.Id})";
                }
                else
                {
                    ProcessStatus = $"Failed to attach - process may not support haptics";
                    AttachButton.IsEnabled = true;
                    RefreshButton.IsEnabled = true;
                    ProcessListBox.IsEnabled = true;
                }
            }
            else
            {
                Detach();
            }
        }

        private void RefreshButton_Click(object aObj, System.Windows.RoutedEventArgs aEvent)
        {
            RunEnumProcessUpdate();
        }

        private void Detach()
        {
            _easyHookMod.Detach();
            _easyHookMod = null;
            Attached = false;
        }
    }
}
