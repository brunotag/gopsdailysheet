using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GopsDailySheet
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            webViewGops.Source = new Uri(
                string.Format(
                    "https://www.glidingops.com/DailySheet?org={0}&key={1}",
                    System.Configuration.ConfigurationSettings.AppSettings.Get("org"),
                    System.Configuration.ConfigurationSettings.AppSettings.Get("key")
                )
            );
            webViewGops.ZoomFactor = double.Parse(System.Configuration.ConfigurationSettings.AppSettings.Get("gopsZoomFactor"));

            webViewGopsTracking.Source = new Uri(
               "https://www.glidingops.com/wgc"
            );

            webViewTracking.Source = new Uri(
               "https://gliding.net.nz/tracking"
            );
        }

        protected override void WndProc(ref Message m)
        {
            if (InhibitDoubleClickResizeOnTitleBar(ref m))
            {
                return;
            }
            base.WndProc(ref m);
        }

        private bool InhibitDoubleClickResizeOnTitleBar(ref Message m)
        {
            const int WM_NCLBUTTONDBLCLK = 0x00A3; //double click on a title bar a.k.a. non-client area of the form
            if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                m.Result = IntPtr.Zero;
                return true;
            }
            return false;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var selectedBrowser = tabControl1.SelectedTab.Controls.OfType<Microsoft.Web.WebView2.WinForms.WebView2>().First();
            selectedBrowser.Reload();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var response = MessageBox.Show("Are you sure you want to quit?", "Quit?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (response == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
