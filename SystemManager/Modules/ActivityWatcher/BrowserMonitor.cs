using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SystemManager.Modules.ActivityWatcher
{
    /// <summary>
    /// Monitors browser activity and tracks URLs by domain
    /// </summary>
    public class BrowserMonitor
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private readonly Dictionary<string, DomainActivity> _domainActivities;
        private readonly List<string> _browserProcessNames = new List<string> 
        { 
            "chrome", "firefox", "msedge", "opera", "brave", "iexplore", "safari" 
        };

        private System.Timers.Timer _monitorTimer;
        private DateTime _sessionStart;
        private string _lastUrl;

        public BrowserMonitor()
        {
            _domainActivities = new Dictionary<string, DomainActivity>();
            _sessionStart = DateTime.Now;
        }

        public void Start()
        {
            _monitorTimer = new System.Timers.Timer(1000); // Check every second
            _monitorTimer.Elapsed += MonitorTimer_Elapsed;
            _monitorTimer.Start();
        }

        public void Stop()
        {
            _monitorTimer?.Stop();
            _monitorTimer?.Dispose();
        }

        private void MonitorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                IntPtr handle = GetForegroundWindow();
                uint processId;
                GetWindowThreadProcessId(handle, out processId);

                var process = Process.GetProcessById((int)processId);
                if (IsBrowserProcess(process.ProcessName))
                {
                    StringBuilder title = new StringBuilder(256);
                    GetWindowText(handle, title, title.Capacity);
                    
                    string windowTitle = title.ToString();
                    string url = ExtractUrlFromTitle(windowTitle);
                    
                    if (!string.IsNullOrEmpty(url) && url != _lastUrl)
                    {
                        _lastUrl = url;
                        string domain = ExtractDomain(url);
                        TrackDomainActivity(domain);
                    }
                }
            }
            catch
            {
                // Silently handle errors
            }
        }

        private bool IsBrowserProcess(string processName)
        {
            return _browserProcessNames.Any(b => processName.ToLower().Contains(b));
        }

        private string ExtractUrlFromTitle(string title)
        {
            // Extract URL from browser window title
            // Most browsers show URL in title or can be extracted
            if (title.Contains("http://") || title.Contains("https://"))
            {
                int start = title.IndexOf("http");
                int end = title.IndexOf(" ", start);
                if (end == -1) end = title.Length;
                return title.Substring(start, end - start);
            }
            return string.Empty;
        }

        private string ExtractDomain(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return "unknown";
            }
        }

        private void TrackDomainActivity(string domain)
        {
            if (!_domainActivities.ContainsKey(domain))
            {
                _domainActivities[domain] = new DomainActivity
                {
                    Domain = domain,
                    FirstSeen = DateTime.Now
                };
            }

            var activity = _domainActivities[domain];
            activity.LastSeen = DateTime.Now;
            activity.TotalSeconds += 1;
            activity.VisitCount++;
        }

        public Dictionary<string, DomainActivity> GetDomainActivities()
        {
            return new Dictionary<string, DomainActivity>(_domainActivities);
        }

        public TimeSpan GetSessionUptime()
        {
            return DateTime.Now - _sessionStart;
        }

        public List<DailyLog> GetDailyLogs()
        {
            var logs = new List<DailyLog>();
            var groupedByDate = _domainActivities.Values
                .GroupBy(d => d.FirstSeen.Date)
                .OrderByDescending(g => g.Key);

            foreach (var group in groupedByDate)
            {
                logs.Add(new DailyLog
                {
                    Date = group.Key,
                    TotalActiveTime = TimeSpan.FromSeconds(group.Sum(d => d.TotalSeconds)),
                    Domains = group.ToDictionary(d => d.Domain, d => TimeSpan.FromSeconds(d.TotalSeconds))
                });
            }

            return logs;
        }
    }

    public class DomainActivity
    {
        public string Domain { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public long TotalSeconds { get; set; }
        public int VisitCount { get; set; }

        public TimeSpan TotalTime => TimeSpan.FromSeconds(TotalSeconds);
    }

    public class DailyLog
    {
        public DateTime Date { get; set; }
        public TimeSpan TotalActiveTime { get; set; }
        public Dictionary<string, TimeSpan> Domains { get; set; }
    }
}
