using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            Application.Run(new mainForm());
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
            Application.Restart();
        }
    }
}
