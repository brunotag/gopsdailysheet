using GopsDailySheet.Config;
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

        #region LoadPresentationFromAppConfig

        private void mainForm_Load(object sender, EventArgs e)
        {
            CleanUpTabs();
            var tabsConfigSection = ConfigurationManager.GetSection("tabsConfigs") as TabsConfigSection;
            BuildTabsFromConfig(tabsConfigSection.Tabs);
            var fontSize = float.Parse(ConfigurationManager.AppSettings.Get("fontSize") ?? "18");
            ReplaceFontSize(fontSize);
        }

        private void CleanUpTabs()
        {
            this.tabControl1.TabPages.Clear();
        }

        private void BuildTabsFromConfig(TabsElementCollection tabs)
        {
            var counter = 0;
            foreach(var tab in tabs.OfType<TabElement>())
            {
                var tabPage = new TabPage();
                tabPage.BackColor = Color.DimGray;
                tabPage.ForeColor = SystemColors.Desktop;                
                tabPage.Padding = new Padding(4);
                //tabPage.Location = new System.Drawing.Point(4, 45);
                //tabPage.Size = new System.Drawing.Size(914, 310);
                tabPage.TabIndex = counter;
                tabPage.Name = "tab" + tab.Name;
                tabPage.Text = tab.Caption;

                var browser = new Microsoft.Web.WebView2.WinForms.WebView2();
                ((ISupportInitialize)(browser)).BeginInit();
                browser.CreationProperties = null;
                browser.DefaultBackgroundColor = Color.White;
                browser.Dock = DockStyle.Fill;
                browser.Location = new Point(0, 0);
                browser.Margin = new Padding(6, 7, 6, 7);
                browser.Name = "webView" + tab.Name;
                //this.webViewGopsTracking.TabIndex = 3;
                browser.ZoomFactor = tab.ZoomFactor ?? 1D;
                browser.Source = new Uri(tab.Url);
                
                tabControl1.Controls.Add(tabPage);            
                tabPage.Controls.Add(browser);
                ((ISupportInitialize)(browser)).EndInit();
                counter++;
            }
        }

        private void ReplaceFontSize(float fontSize)
        {
            tabControl1.Font = tabControl1.Font.CloneWithNewSize(fontSize);
            refreshToolStripButton.Font = refreshToolStripButton.Font.CloneWithNewSize(fontSize);
            toolStripDropDownButtonQuit.Font = toolStripDropDownButtonQuit.Font.CloneWithNewSize(fontSize);
            this.Font = this.Font.CloneWithNewSize(fontSize);
        }

        #endregion

        #region InhibitResizeByDoubleClick
        protected override void WndProc(ref Message m)
        {
            if (ShouldInhibitBecauseDoubleClickOnTitleBar(ref m))
            {
                return;
            }
            base.WndProc(ref m);
        }

        private static bool ShouldInhibitBecauseDoubleClickOnTitleBar(ref Message m)
        {
            const int WM_NCLBUTTONDBLCLK = 0x00A3;
            if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                m.Result = IntPtr.Zero;
                return true;
            }
            return false;
        }
        #endregion

        #region FunctionalityByEventHandlers

        private void refreshToolStripButton_Click(object sender, EventArgs e)
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

        #endregion

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var response = MessageBox.Show("Are you sure you want to restart?", "Restart?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (response == DialogResult.Yes)
            {
                Program.Restart();
            }
        }
    }
}
