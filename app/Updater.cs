namespace GopsDailySheet
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Reflection;
    using System.Windows.Forms;
    using Newtonsoft.Json;

    public class Updater
    {
        private static readonly string updateUrl = "https://github.com/brunotag/gopsdailysheet/releases/latest/download/latest.json";
        private static readonly string downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip");

        public static void CheckForUpdates()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string json = client.DownloadString(updateUrl);
                    UpdateInfo updateInfo = JsonConvert.DeserializeObject<UpdateInfo>(json);
                    string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                    if (Version.Parse(updateInfo.version) > Version.Parse(currentVersion))
                    {
                        DialogResult result = MessageBox.Show(
                            $"A new version {updateInfo.version} is available. Do you want to update now?",
                            "Update Available",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            DownloadUpdate(updateInfo.download_url);
                            ApplyUpdate();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update check failed: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void DownloadUpdate(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, downloadPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Download failed: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ApplyUpdate()
        {
            try
            {
                // Get the path of the executing assembly (the running executable)
                string appPath = AppDomain.CurrentDomain.BaseDirectory;

                // Get the name of the executable
                string appName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

                // Define log file path
                //string logFilePath = Path.Combine(appPath, "update_log.txt");

                // Build the PowerShell command to invoke the script and redirect output
                string psArguments = $"-NoExit -ExecutionPolicy Bypass -File update.ps1 -AppName \"{appName}\" -ExtractPath \"{appPath}\"";
                
                //this trim shouldn't be required.
                psArguments = psArguments.Trim('\"');

                // Pass the application name and path to the PowerShell script
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = psArguments,
                    WorkingDirectory = appPath,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                // Start the batch file and close the app
                Process.Start(psi);

                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class UpdateInfo
    {
        public string version { get; set; }
        public string download_url { get; set; }
    }

}
