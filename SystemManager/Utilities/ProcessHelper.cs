using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace SystemManager.Utilities
{
    /// <summary>
    /// Helper class for process management
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Get all running processes with details
        /// </summary>
        public static List<ProcessInfo> GetRunningProcesses()
        {
            var processList = new List<ProcessInfo>();

            try
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        processList.Add(new ProcessInfo
                        {
                            ProcessId = process.Id,
                            ProcessName = process.ProcessName,
                            WindowTitle = process.MainWindowTitle,
                            MemoryUsage = process.WorkingSet64,
                            StartTime = process.StartTime
                        });
                    }
                    catch
                    {
                        // Some processes may not be accessible
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }

            return processList;
        }

        /// <summary>
        /// Get process command line
        /// </summary>
        public static string GetProcessCommandLine(int processId)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["CommandLine"]?.ToString();
                    }
                }
            }
            catch
            {
                // Handle exceptions
            }

            return string.Empty;
        }

        /// <summary>
        /// Terminate a process safely
        /// </summary>
        public static bool TerminateProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ProcessInfo
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }
        public long MemoryUsage { get; set; }
        public DateTime StartTime { get; set; }
    }
}
