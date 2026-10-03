using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace GopsDailySheet
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (IsAlreadyRunning())
            {
                MessageBox.Show("GopsDailySheet is already running.", "Max one instance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnThreadException;
            Application.Run(new mainForm());
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (FindWebView2RuntimeException(e.Exception) != null)
            {
                MessageBox.Show(
                    "The Microsoft Edge WebView2 Runtime is required to display the tabs in this app and is not installed." +
                    Environment.NewLine + Environment.NewLine +
                    "Install it from https://developer.microsoft.com/microsoft-edge/webview2/ and start this app again.",
                    "Missing WebView2 Runtime", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            MessageBox.Show(e.Exception.Message, "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static WebView2RuntimeNotFoundException FindWebView2RuntimeException(Exception exception)
        {
            while (exception != null)
            {
                var runtimeException = exception as WebView2RuntimeNotFoundException;
                if (runtimeException != null)
                {
                    return runtimeException;
                }

                exception = exception.InnerException;
            }

            return null;
        }

        private static Mutex _mutex;
        static bool IsAlreadyRunning()
        {
            const string appName = "GopsDailySheet";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            return !createdNew;
        }

        internal static void Restart()
        {
            _mutex.ReleaseMutex();
            _mutex.Close();
            _mutex.Dispose();

            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(new ProcessStartInfo(exePath) { WorkingDirectory = Path.GetDirectoryName(exePath) });
            Application.Exit();
        }
    }
}