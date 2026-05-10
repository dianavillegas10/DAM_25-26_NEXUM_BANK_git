namespace NexumApp.Forms.Principal
{
    partial class FrmDashboard
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlHost;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHost = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // pnlHost
            // 
            this.pnlHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHost.Location = new System.Drawing.Point(0, 0);
            this.pnlHost.Name = "pnlHost";
            this.pnlHost.Size = new System.Drawing.Size(1366, 768);
            this.pnlHost.TabIndex = 0;
            // 
            // FrmDashboard
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1366, 768);
            this.Controls.Add(this.pnlHost);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(1200, 700);
            this.Name = "FrmDashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nexum Bank Dashboard";
            this.Load += new System.EventHandler(this.FrmDashboard_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragArea_MouseDown);
            this.ResumeLayout(false);
        }
    }
}
