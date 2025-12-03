using System;
using System.Management;

namespace SystemManager.Modules.SystemCleaning
{
    /// <summary>
    /// Manages system restore points using WMI
    /// </summary>
    public class RestorePointManager
    {
        public bool CreateRestorePoint(string description)
        {
            try
            {
                var scope = new ManagementScope("\\\\localhost\\root\\default");
                var path = new ManagementPath("SystemRestore");
                var options = new ObjectGetOptions();
                
                using (var systemRestore = new ManagementClass(scope, path, options))
                {
                    var parameters = systemRestore.GetMethodParameters("CreateRestorePoint");
                    parameters["Description"] = description;
                    parameters["RestorePointType"] = 12; // MODIFY_SETTINGS
                    parameters["EventType"] = 100; // BEGIN_SYSTEM_CHANGE

                    var result = systemRestore.InvokeMethod("CreateRestorePoint", parameters, null);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool BackupRegistry(string backupPath)
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "reg.exe",
                        Arguments = $"export HKLM \"{backupPath}\\HKLM_Backup.reg\" /y",
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
}
