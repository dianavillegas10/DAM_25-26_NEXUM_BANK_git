using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NexumApp.Forms.Principal
{
    internal partial class FrmDashboard : Form
    {
        private FrmDashboardUsuario _dashboardUsuario;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        public FrmDashboard()
        {
            InitializeComponent();
        }

        private void FrmDashboard_Load(object sender, EventArgs e)
        {
            if (_dashboardUsuario == null)
            {
                _dashboardUsuario = new FrmDashboardUsuario
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill
                };
                pnlHost.Controls.Clear();
                pnlHost.Controls.Add(_dashboardUsuario);
                _dashboardUsuario.Show();
            }
        }

        private void DragArea_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }
    }
}