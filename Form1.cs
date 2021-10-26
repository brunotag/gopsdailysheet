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
    }
}
