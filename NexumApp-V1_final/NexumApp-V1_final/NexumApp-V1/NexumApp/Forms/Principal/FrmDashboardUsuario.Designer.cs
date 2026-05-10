using NexumApp.Helpers;

namespace NexumApp.Forms.Principal
{
    partial class FrmDashboardUsuario
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.TableLayoutPanel tlpAreaPrincipal;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlContenido;
        private System.Windows.Forms.Panel pnlFooter;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.tlpAreaPrincipal = new System.Windows.Forms.TableLayoutPanel();
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlContenido = new System.Windows.Forms.Panel();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.tlpRoot.SuspendLayout();
            this.tlpAreaPrincipal.SuspendLayout();
            this.SuspendLayout();
            // tlpRoot
            this.tlpRoot.BackColor = DashboardLayout.FondoPrincipal;
            this.tlpRoot.ColumnCount = 2;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, DashboardLayout.SidebarWidth));
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.pnlSidebar, 0, 0);
            this.tlpRoot.Controls.Add(this.tlpAreaPrincipal, 1, 0);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(0, 0);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.RowCount = 1;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Size = new System.Drawing.Size(1280, 770);
            this.tlpRoot.TabIndex = 0;
            // tlpAreaPrincipal
            this.tlpAreaPrincipal.BackColor = DashboardLayout.FondoPrincipal;
            this.tlpAreaPrincipal.ColumnCount = 1;
            this.tlpAreaPrincipal.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAreaPrincipal.Controls.Add(this.pnlHeader, 0, 0);
            this.tlpAreaPrincipal.Controls.Add(this.pnlContenido, 0, 1);
            this.tlpAreaPrincipal.Controls.Add(this.pnlFooter, 0, 2);
            this.tlpAreaPrincipal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpAreaPrincipal.Location = new System.Drawing.Point(220, 0);
            this.tlpAreaPrincipal.Margin = new System.Windows.Forms.Padding(0);
            this.tlpAreaPrincipal.Name = "tlpAreaPrincipal";
            this.tlpAreaPrincipal.RowCount = 3;
            this.tlpAreaPrincipal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpAreaPrincipal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAreaPrincipal.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpAreaPrincipal.Size = new System.Drawing.Size(1060, 770);
            this.tlpAreaPrincipal.TabIndex = 1;
            // pnlSidebar
            this.pnlSidebar.BackColor = DashboardLayout.SidebarFondo;
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSidebar.Margin = new System.Windows.Forms.Padding(0);
            this.pnlSidebar.MinimumSize = new System.Drawing.Size(DashboardLayout.SidebarWidth, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(DashboardLayout.SidebarWidth, 770);
            this.pnlSidebar.TabIndex = 0;
            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(0);
            this.pnlHeader.MinimumSize = new System.Drawing.Size(0, 100);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1060, 100);
            this.pnlHeader.TabIndex = 1;
            // pnlContenido
            this.pnlContenido.BackColor = DashboardLayout.FondoPrincipal;
            this.pnlContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContenido.Margin = new System.Windows.Forms.Padding(0);
            this.pnlContenido.Padding = new System.Windows.Forms.Padding(DashboardLayout.Margen);
            this.pnlContenido.Name = "pnlContenido";
            this.pnlContenido.Size = new System.Drawing.Size(1060, 626);
            this.pnlContenido.TabIndex = 2;
            // pnlFooter
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(249, 250, 251);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFooter.Margin = new System.Windows.Forms.Padding(0);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(1060, 44);
            this.pnlFooter.TabIndex = 3;
            // FrmDashboardUsuario
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 770);
            this.Controls.Add(this.tlpRoot);
            this.MinimumSize = new System.Drawing.Size(1200, 700);
            this.Name = "FrmDashboardUsuario";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nexum Bank - Mi Banca";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmDashboardUsuario_Load);
            this.tlpRoot.ResumeLayout(false);
            this.tlpAreaPrincipal.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
