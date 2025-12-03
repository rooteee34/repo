using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;

namespace SystemManager.Modules.NetworkMonitor
{
    /// <summary>
    /// Monitors network signal strength, speed, and stability
    /// </summary>
    public class SignalStrengthMonitor
    {
        private readonly List<NetworkSample> _samples;
        private readonly object _lock = new object();
        private System.Timers.Timer _monitorTimer;
        private long _lastBytesReceived;
        private long _lastBytesSent;
        private DateTime _lastSampleTime;

        public SignalStrengthMonitor()
        {
            _samples = new List<NetworkSample>();
            _lastSampleTime = DateTime.Now;
        }

        public void Start()
        {
            _monitorTimer = new System.Timers.Timer(1000); // Sample every second
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
                var sample = CollectSample();
                if (sample != null)
                {
                    lock (_lock)
                    {
                        _samples.Add(sample);
                        
                        // Keep only last 3600 samples (1 hour at 1 sample/sec)
                        if (_samples.Count > 3600)
                        {
                            _samples.RemoveAt(0);
                        }
                    }
                }
            }
            catch
            {
                // Silently handle errors
            }
        }

        private NetworkSample CollectSample()
        {
            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(a => a.OperationalStatus == OperationalStatus.Up && 
                               a.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .ToList();

                if (adapters.Count == 0)
                    return null;

                long totalBytesReceived = 0;
                long totalBytesSent = 0;

                foreach (var adapter in adapters)
                {
                    var stats = adapter.GetIPStatistics();
                    totalBytesReceived += stats.BytesReceived;
                    totalBytesSent += stats.BytesSent;
                }

                var now = DateTime.Now;
                var timeDiff = (now - _lastSampleTime).TotalSeconds;

                var sample = new NetworkSample
                {
                    Timestamp = now,
                    DownloadSpeed = timeDiff > 0 ? (totalBytesReceived - _lastBytesReceived) / timeDiff : 0,
                    UploadSpeed = timeDiff > 0 ? (totalBytesSent - _lastBytesSent) / timeDiff : 0,
                    SignalStrength = GetWiFiSignalStrength()
                };

                _lastBytesReceived = totalBytesReceived;
                _lastBytesSent = totalBytesSent;
                _lastSampleTime = now;

                return sample;
            }
            catch
            {
                return null;
            }
        }

        private int GetWiFiSignalStrength()
        {
            try
            {
                // Use WMI to get WiFi signal strength
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM MSNdis_80211_ReceivedSignalStrength"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var rssi = Convert.ToInt32(obj["Ndis80211ReceivedSignalStrength"]);
                        // Convert RSSI to percentage (rough approximation)
                        // RSSI typically ranges from -100 (worst) to -50 (best)
                        int percentage = Math.Min(100, Math.Max(0, 2 * (rssi + 100)));
                        return percentage;
                    }
                }
            }
            catch
            {
                // If WMI fails, return -1 to indicate unavailable
            }

            return -1;
        }

        public NetworkStatistics GetCurrentStatistics()
        {
            lock (_lock)
            {
                if (_samples.Count == 0)
                    return new NetworkStatistics();

                var recent = _samples.TakeLast(60).ToList(); // Last 60 seconds

                return new NetworkStatistics
                {
                    CurrentDownloadSpeed = recent.LastOrDefault()?.DownloadSpeed ?? 0,
                    CurrentUploadSpeed = recent.LastOrDefault()?.UploadSpeed ?? 0,
                    CurrentSignalStrength = recent.LastOrDefault()?.SignalStrength ?? -1,
                    AverageDownloadSpeed = recent.Average(s => s.DownloadSpeed),
                    AverageUploadSpeed = recent.Average(s => s.UploadSpeed),
                    PeakDownloadSpeed = recent.Max(s => s.DownloadSpeed),
                    PeakUploadSpeed = recent.Max(s => s.UploadSpeed),
                    Stability = CalculateStability(recent),
                    Jitter = CalculateJitter(recent)
                };
            }
        }

        private double CalculateStability(List<NetworkSample> samples)
        {
            if (samples.Count < 2)
                return 100;

            // Calculate standard deviation of download speed
            var speeds = samples.Select(s => s.DownloadSpeed).ToList();
            var average = speeds.Average();
            var variance = speeds.Sum(s => Math.Pow(s - average, 2)) / speeds.Count;
            var stdDev = Math.Sqrt(variance);

            // Convert to stability percentage (lower stdDev = higher stability)
            var stability = Math.Max(0, 100 - (stdDev / (average + 1) * 100));
            return Math.Min(100, stability);
        }

        private double CalculateJitter(List<NetworkSample> samples)
        {
            if (samples.Count < 2)
                return 0;

            // Calculate average difference between consecutive samples
            double totalDiff = 0;
            for (int i = 1; i < samples.Count; i++)
            {
                totalDiff += Math.Abs(samples[i].DownloadSpeed - samples[i - 1].DownloadSpeed);
            }

            return totalDiff / (samples.Count - 1);
        }

        public List<NetworkSample> GetSamples(int count = 60)
        {
            lock (_lock)
            {
                return _samples.TakeLast(count).ToList();
            }
        }
    }

    public class NetworkSample
    {
        public DateTime Timestamp { get; set; }
        public double DownloadSpeed { get; set; } // bytes per second
        public double UploadSpeed { get; set; } // bytes per second
        public int SignalStrength { get; set; } // percentage (0-100) or -1 if unavailable
    }

    public class NetworkStatistics
    {
        public double CurrentDownloadSpeed { get; set; }
        public double CurrentUploadSpeed { get; set; }
        public int CurrentSignalStrength { get; set; }
        public double AverageDownloadSpeed { get; set; }
        public double AverageUploadSpeed { get; set; }
        public double PeakDownloadSpeed { get; set; }
        public double PeakUploadSpeed { get; set; }
        public double Stability { get; set; } // percentage
        public double Jitter { get; set; }

        public string FormatSpeed(double bytesPerSecond)
        {
            if (bytesPerSecond < 1024)
                return $"{bytesPerSecond:F2} B/s";
            else if (bytesPerSecond < 1024 * 1024)
                return $"{bytesPerSecond / 1024:F2} KB/s";
            else if (bytesPerSecond < 1024 * 1024 * 1024)
                return $"{bytesPerSecond / (1024 * 1024):F2} MB/s";
            else
                return $"{bytesPerSecond / (1024 * 1024 * 1024):F2} GB/s";
        }
    }
}
