using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Security;

namespace SystemManager.Utilities
{
    /// <summary>
    /// Helper class for Windows Registry operations
    /// </summary>
    public static class RegistryHelper
    {
        /// <summary>
        /// Get startup programs from Registry
        /// </summary>
        public static List<StartupEntry> GetStartupPrograms()
        {
            var startupList = new List<StartupEntry>();

            try
            {
                // HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
                AddStartupEntries(startupList, Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKCU");

                // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
                AddStartupEntries(startupList, Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKLM");

                // HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce
                AddStartupEntries(startupList, Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\RunOnce", "HKCU");

                // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce
                AddStartupEntries(startupList, Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", "HKLM");
            }
            catch (SecurityException)
            {
                // Handle security exceptions for registry access
            }
            catch (Exception)
            {
                // Handle other exceptions
            }

            return startupList;
        }

        private static void AddStartupEntries(List<StartupEntry> list, RegistryKey rootKey, string subKeyPath, string hive)
        {
            try
            {
                using (var key = rootKey.OpenSubKey(subKeyPath))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            var value = key.GetValue(valueName)?.ToString();
                            if (!string.IsNullOrEmpty(value))
                            {
                                list.Add(new StartupEntry
                                {
                                    Name = valueName,
                                    Command = value,
                                    Location = $"{hive}\\{subKeyPath}",
                                    Type = "Registry"
                                });
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silently handle errors for individual keys
            }
        }

        /// <summary>
        /// Backup registry key
        /// </summary>
        public static bool BackupRegistryKey(string keyPath, string backupFile)
        {
            try
            {
                // Implementation would use reg export command or native API
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "reg.exe",
                        Arguments = $"export \"{keyPath}\" \"{backupFile}\" /y",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }

    public class StartupEntry
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }
    }
}
