using GopsDailySheet.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GopsDailySheet
{
    public partial class mainForm : Form
    {
        private static readonly string userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GopsDailySheet",
            "EBWebView");

        private const int WM_DISPLAYCHANGE = 0x007E;
        private const int WM_POWERBROADCAST = 0x0218;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;
        private const int SC_RESTORE = 0xF120;
        private const int SC_MONITORPOWER = 0xF170;
        private const int SC_SCREENSAVE = 0xF140;
        private const int PBT_APMPOWERSTATUSCHANGE = 0x0006;
        private const int PBT_APMSUSPEND = 0x0004;
        private const int PBT_APMRESUMEAUTOMATIC = 0x0012;

        private readonly System.Windows.Forms.Timer focusWatcher = new System.Windows.Forms.Timer();
        private readonly System.Windows.Forms.Timer autoSwitchTimer = new System.Windows.Forms.Timer();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr window, StringBuilder text, int maxCount);

        public mainForm()
        {
            InitializeComponent();
            focusWatcher.Tick += focusWatcher_Tick;
            autoSwitchTimer.Tick += autoSwitchTimer_Tick;
            countdownTimer.Tick += CountdownTimer_Tick;
        }

        #region FocusDiagnostics

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Log.Write("gained foreground");
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            Log.Write("deactivated");

            // Something else is taking the foreground. Ask again once things have
            // settled, so the log names whoever ended up in front.
            focusWatcher.Interval = 250;
            focusWatcher.Stop();
            focusWatcher.Start();
        }

        private void focusWatcher_Tick(object sender, EventArgs e)
        {
            focusWatcher.Stop();
            Log.Write("lost foreground to " + DescribeForegroundWindow());
        }

        private static string DescribeForegroundWindow()
        {
            try
            {
                IntPtr window = GetForegroundWindow();
                if (window == IntPtr.Zero) { return "no foreground window"; }

                uint processId;
                GetWindowThreadProcessId(window, out processId);

                var title = new StringBuilder(256);
                GetWindowText(window, title, title.Capacity);

                string name = "pid " + processId;
                try { name = Process.GetProcessById((int)processId).ProcessName + " (pid " + processId + ")"; }
                catch (ArgumentException) { }

                return name + " \"" + title + "\"";
            }
            catch (Exception ex)
            {
                return "could not be identified: " + ex.Message;
            }
        }

        #endregion

        #region LoadPresentationFromAppConfig

        private void mainForm_Load(object sender, EventArgs e)
        {
            this.tabControl1.TabPages.Clear();

            var tabsConfigSection = ConfigurationManager.GetSection("tabsConfigs") as TabsConfigSection;
            BuildTabsFromConfig(tabsConfigSection.Tabs);
            tabControl1.Selecting += TabControl1_Selecting;
            tabControl1.Deselected += TabControl1_Deselected;
            tabControl1.Selected += TabControl1_Selected;
            var fontSize = float.Parse(ConfigurationManager.AppSettings.Get("fontSize") ?? "18");
            ReplaceFontSize(fontSize);
            lbl_version.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void TabControl1_Deselected(object sender, TabControlEventArgs e)
        {
            TabElement tabConfig = (TabElement)e.TabPage.Tag;
            if (tabConfig.UnloadOnLostFocus != null && tabConfig.UnloadOnLostFocus.Value == true)
            {
                e.TabPage.Controls.OfType<Microsoft.Web.WebView2.WinForms.WebView2>().First().Dispose();
                e.TabPage.Controls.Clear();
            }
        }

        private void TabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            TabElement tabConfig = (TabElement)e.TabPage.Tag;
            if (tabConfig.UnloadOnLostFocus != null && tabConfig.UnloadOnLostFocus.Value == true)
            {
                e.TabPage.Controls.Add(GetBrowserControl(tabConfig));
            }
        }

        #region AutoSwitchBackToFirstTab

        private void TabControl1_Selected(object sender, TabControlEventArgs e)
        {
            ScheduleSwitchToFirstTab((TabElement)e.TabPage.Tag);
            UpdateCountdownDisplay();
        }

        private readonly System.Windows.Forms.Timer countdownTimer = new System.Windows.Forms.Timer();
        private int _secondsLeft = 0;

        /// <summary>
        /// A tab may allow itself to stay open for a while and then hand the
        /// screen back to the first tab, so that a visitor cannot strand the
        /// tablet on a tracking page. A negative or missing value never
        /// switches; the first tab itself never switches either, or the timer
        /// would fire forever.
        /// </summary>
        private void ScheduleSwitchToFirstTab(TabElement tabConfig)
        {
            autoSwitchTimer.Stop();
            countdownTimer.Stop();
            _secondsLeft = 0;
            UpdateCountdownDisplay();
            if (tabConfig == null || tabConfig.SwitchToFirstTabAfterSeconds <= 0) { return; }
            if (tabControl1.TabPages.Count < 2 || tabControl1.SelectedTab == tabControl1.TabPages[0]) { return; }

            int seconds = tabConfig.SwitchToFirstTabAfterSeconds;
            _secondsLeft = seconds;
            UpdateCountdownDisplay();

            autoSwitchTimer.Interval = (int)Math.Min((long)seconds * 1000, int.MaxValue);
            autoSwitchTimer.Start();
            countdownTimer.Interval = 1000;
            countdownTimer.Tick -= CountdownTimer_Tick;
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (_secondsLeft <= 0) { countdownTimer.Stop(); UpdateCountdownDisplay(); return; }
            _secondsLeft--;
            UpdateCountdownDisplay();
        }

        private void UpdateCountdownDisplay()
        {
            if (lbl_countdown == null) { return; }
            if (_secondsLeft <= 0) { lbl_countdown.Text = string.Empty; return; }
            lbl_countdown.Text = _secondsLeft.ToString();
        }

        private void autoSwitchTimer_Tick(object sender, EventArgs e)
        {
            autoSwitchTimer.Stop();
            countdownTimer.Stop();
            _secondsLeft = 0;
            UpdateCountdownDisplay();
            var leaving = tabControl1.SelectedTab;
            if (leaving == null || tabControl1.TabPages.Count < 2) { return; }

            var firstTab = tabControl1.TabPages[0];
            var tabConfig = leaving.Tag as TabElement;
            int seconds = tabConfig == null ? 0 : tabConfig.SwitchToFirstTabAfterSeconds;

            tabControl1.SelectedTab = firstTab;
            Log.Write($"'{leaving.Text}' was open for {seconds}s, switched back to '{firstTab.Text}'");
        }

        #endregion

        private void BuildTabsFromConfig(TabsElementCollection tabsConfig)
        {
            var counter = 0;
            foreach(var tabConfig in tabsConfig.OfType<TabElement>())
            {
                var tabPage = new TabPage();
                tabPage.BackColor = Color.DimGray;
                tabPage.ForeColor = SystemColors.Desktop;                
                tabPage.Padding = new Padding(4);
                //tabPage.Location = new System.Drawing.Point(4, 45);
                //tabPage.Size = new System.Drawing.Size(914, 310);
                tabPage.TabIndex = counter;
                tabPage.Name = "tab" + tabConfig.Name;
                tabPage.Text = tabConfig.Caption;
                tabControl1.Controls.Add(tabPage);

                tabPage.Tag = tabConfig;

                if (tabConfig.UnloadOnLostFocus == null || tabConfig.UnloadOnLostFocus.Value == false)
                {
                    tabPage.Controls.Add(GetBrowserControl(tabConfig));
                }

                counter++;
            }
        }

        private Control GetBrowserControl(TabElement tabConfig)
        {
            var browser = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((ISupportInitialize)(browser)).BeginInit();
            browser.CreationProperties = new Microsoft.Web.WebView2.WinForms.CoreWebView2CreationProperties { UserDataFolder = userDataFolder };
            browser.DefaultBackgroundColor = Color.White;
            browser.Dock = DockStyle.Fill;
            browser.Location = new Point(0, 0);
            browser.Margin = new Padding(6, 7, 6, 7);
            browser.Name = "webView" + tabConfig.Name;
            //this.webViewGopsTracking.TabIndex = 3;
            browser.ZoomFactor = tabConfig.ZoomFactor ?? 1D;
            browser.Source = new Uri(tabConfig.Url);
            ((ISupportInitialize)(browser)).EndInit();
            WatchBrowser(tabConfig, browser);
            return browser;
        }

        /// <summary>
        /// Logs what the embedded browser does, so a page that opens a window or
        /// loses its process can be told apart from the app misbehaving.
        /// </summary>
        private static void WatchBrowser(TabElement tabConfig, Microsoft.Web.WebView2.WinForms.WebView2 browser)
        {
            browser.CoreWebView2InitializationCompleted += (sender, e) =>
            {
                if (!e.IsSuccess)
                {
                    Log.Write($"browser for {tabConfig.Name} failed to start: {e.InitializationException}");
                    return;
                }

                var started = (Microsoft.Web.WebView2.WinForms.WebView2)sender;
                started.CoreWebView2.NewWindowRequested += (s, args) => Log.Write($"{tabConfig.Name} wants a new window: {args.Uri}");
                started.CoreWebView2.ProcessFailed += (s, args) => Log.Write($"{tabConfig.Name} browser process failed: {args.ProcessFailedKind}, exit code {args.ExitCode}");
            };
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

            LogSystemMessage(ref m);

            base.WndProc(ref m);
        }

        /// <summary>
        /// Records the rare window messages that explain a whole desktop that
        /// minimises by itself, so a driver reset can be told apart from a
        /// Show desktop or a sleeping monitor.
        /// </summary>
        private static void LogSystemMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DISPLAYCHANGE:
                    Log.Write("display settings changed (resolution or colour depth) - display driver reset or monitor reconnect");
                    break;
                case WM_POWERBROADCAST:
                    Log.Write("power broadcast " + m.LParam + " (" + DescribePowerBroadcast((int)m.LParam) + ")");
                    break;
                case WM_SYSCOMMAND:
                    Log.Write("system command " + DescribeSystemCommand((int)(m.WParam.ToInt64() & 0xFFF0)));
                    break;
            }
        }

        private static string DescribePowerBroadcast(int lParam)
        {
            switch (lParam)
            {
                case PBT_APMRESUMEAUTOMATIC: return "resuming automatically";
                case PBT_APMSUSPEND: return "suspending";
                case PBT_APMPOWERSTATUSCHANGE: return "power status changed";
                default: return "unhandled";
            }
        }

        private static string DescribeSystemCommand(int command)
        {
            switch (command)
            {
                case SC_MINIMIZE: return "minimize";
                case SC_MAXIMIZE: return "maximize";
                case SC_RESTORE: return "restore";
                case SC_MONITORPOWER: return "monitor power";
                case SC_SCREENSAVE: return "screen saver";
                default: return "0x" + command.ToString("X");
            }
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
