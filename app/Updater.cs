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
        private static readonly string extractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update");

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
                string batchFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.bat");

                // Write a batch script to safely replace files and restart the app
                File.WriteAllText(batchFilePath, $@"
                @echo off
                timeout /t 2 /nobreak >nul
                del ""{Application.ExecutablePath}""
                powershell -Command ""Expand-Archive -Path '{downloadPath}' -DestinationPath '{extractPath}' -Force""
                move /Y ""{extractPath}\*"" ""{AppDomain.CurrentDomain.BaseDirectory}""
                start """" ""{Path.GetFileName(Application.ExecutablePath)}""
                del release.zip
                del update.bat
                exit");

                // Start the batch file and close the app
                Process.Start(new ProcessStartInfo
                {
                    FileName = batchFilePath,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

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
