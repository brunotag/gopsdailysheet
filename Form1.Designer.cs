namespace GopsDailySheet
{
    partial class mainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainForm));
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.webViewGops = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.webViewTracking = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.webViewGopsTracking = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webViewGops)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webViewTracking)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webViewGopsTracking)).BeginInit();
            this.SuspendLayout();
            // 
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
            // 
            // webViewGops
            // 
            this.webViewGops.CreationProperties = null;
            this.webViewGops.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewGops.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewGops.Location = new System.Drawing.Point(6, 7);
            this.webViewGops.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.webViewGops.Name = "webViewGops";
            this.webViewGops.Size = new System.Drawing.Size(1847, 934);
            this.webViewGops.TabIndex = 1;
            this.webViewGops.ZoomFactor = 1D;
            // 
            // tabControl1
            // 
            this.tabControl1.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1867, 993);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Menu;
            this.tabPage1.Controls.Add(this.webViewGops);
            this.tabPage1.ForeColor = System.Drawing.SystemColors.Desktop;
            this.tabPage1.Location = new System.Drawing.Point(4, 41);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.tabPage1.Size = new System.Drawing.Size(1859, 948);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "GOPS Daily Sheet";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Menu;
            this.tabPage2.Controls.Add(this.webViewTracking);
            this.tabPage2.Location = new System.Drawing.Point(4, 41);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.tabPage2.Size = new System.Drawing.Size(1859, 948);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Gliding.Net Tracking";
            // 
            // webViewTracking
            // 
            this.webViewTracking.CreationProperties = null;
            this.webViewTracking.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewTracking.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewTracking.Location = new System.Drawing.Point(6, 7);
            this.webViewTracking.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.webViewTracking.Name = "webViewTracking";
            this.webViewTracking.Size = new System.Drawing.Size(1847, 934);
            this.webViewTracking.TabIndex = 2;
            this.webViewTracking.ZoomFactor = 1D;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Menu;
            this.tabPage3.Controls.Add(this.webViewGopsTracking);
            this.tabPage3.Location = new System.Drawing.Point(4, 41);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1859, 948);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "GOPS Tracking";
            // 
            // webViewGopsTracking
            // 
            this.webViewGopsTracking.CreationProperties = null;
            this.webViewGopsTracking.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webViewGopsTracking.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewGopsTracking.Location = new System.Drawing.Point(0, 0);
            this.webViewGopsTracking.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.webViewGopsTracking.Name = "webViewGopsTracking";
            this.webViewGopsTracking.Size = new System.Drawing.Size(1859, 948);
            this.webViewGopsTracking.TabIndex = 3;
            this.webViewGopsTracking.ZoomFactor = 1D;
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1867, 993);
            this.ControlBox = false;
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.Name = "mainForm";
            this.Text = "WWGC Tablet";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.mainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.webViewGops)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.webViewTracking)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.webViewGopsTracking)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PrintDialog printDialog1;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewGops;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewTracking;
        private System.Windows.Forms.TabPage tabPage3;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewGopsTracking;
    }
}

