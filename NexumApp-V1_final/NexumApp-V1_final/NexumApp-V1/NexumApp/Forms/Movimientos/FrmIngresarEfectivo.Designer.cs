namespace NexumApp.Forms.Movimientos
{
    partial class FrmIngresarEfectivo
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
            this.ClientSize = new System.Drawing.Size(500, 660);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmIngresarEfectivo";
            this.Text = "Ingresar efectivo — Nexum Bank";
            this.Load += new System.EventHandler(this.FrmIngresarEfectivo_Load);
            this.ResumeLayout(false);
        }
    }
}
