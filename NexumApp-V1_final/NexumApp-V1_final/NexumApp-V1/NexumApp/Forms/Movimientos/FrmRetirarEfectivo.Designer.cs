namespace NexumApp.Forms.Movimientos
{
    partial class FrmRetirarEfectivo
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 690);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmRetirarEfectivo";
            this.Text = "Retirar efectivo — Nexum Bank";
            this.Load += new System.EventHandler(this.FrmRetirarEfectivo_Load);
            this.ResumeLayout(false);
        }
    }
}
