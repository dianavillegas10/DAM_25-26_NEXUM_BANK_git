using NexumApp.Services;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NexumApp.Forms.Auth
{
    /// <summary>
    /// Formulario de registro de nuevos usuarios en NexumApp.
    /// Permite crear cuentas con validación en tiempo real de todos los campos.
    /// </summary>
    /// <remarks>
    /// Características principales:
    /// - Validación de email con expresión regular
    /// - Indicador de fortaleza de contraseña en tiempo real
    /// - Verificación de coincidencia de contraseñas
    /// - Comprobación de email duplicado antes del registro
    /// - Sistema de notificaciones moderno (toast + errores inline)
    /// - Diseño coherente con FrmLogin (panel dividido, mismos colores)
    /// </remarks>
    public partial class FrmRegistro : Form
    {
        /// <summary>Servicio de autenticación para registrar usuarios y verificar emails.</summary>
        private readonly AuthService _authService = new AuthService();

        #region Definición de colores del tema

        /// <summary>Color de fondo normal para los campos de texto.</summary>
        private readonly Color colorFondoCampo = Color.FromArgb(35, 35, 40);

        /// <summary>Color de fondo cuando el campo tiene foco.</summary>
        private readonly Color colorFondoCampoFocus = Color.FromArgb(45, 45, 50);

        /// <summary>Color de fondo cuando hay error en el campo.</summary>
        private readonly Color colorFondoCampoError = Color.FromArgb(60, 35, 35);

        /// <summary>Color de fondo cuando el campo es válido (contraseñas coinciden).</summary>
        private readonly Color colorFondoCampoExito = Color.FromArgb(35, 55, 40);

        /// <summary>Color rojo para notificaciones de error.</summary>
        private readonly Color colorNotificacionError = Color.FromArgb(220, 53, 69);

        /// <summary>Color amarillo para notificaciones de advertencia.</summary>
        private readonly Color colorNotificacionWarning = Color.FromArgb(255, 193, 7);

        /// <summary>Color azul para notificaciones informativas.</summary>
        private readonly Color colorNotificacionInfo = Color.FromArgb(23, 162, 184);

        /// <summary>Color verde para notificaciones de éxito.</summary>
        private readonly Color colorNotificacionExito = Color.FromArgb(40, 167, 69);

        #endregion

        #region Sistema de animación de notificaciones

        /// <summary>Timer que controla la animación de altura del panel de notificación.</summary>
        private Timer timerAnimacion;

        /// <summary>Altura objetivo para la animación (0 = oculto, ALTURA_NOTIFICACION = visible).</summary>
        private int alturaObjetivo = 0;

        /// <summary>Altura en píxeles del panel de notificación cuando está visible.</summary>
        private const int ALTURA_NOTIFICACION = 50;

        #endregion

        /// <summary>
        /// Constructor del formulario de registro.
        /// Configura el doble buffer para evitar parpadeos en la animación.
        /// </summary>
        public FrmRegistro()
        {
            InitializeComponent();

            // Habilita doble buffer para animaciones suaves sin parpadeo
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            // Configura el timer de animación para el panel de notificaciones
            timerAnimacion = new Timer();
            timerAnimacion.Interval = 15; // 15ms = ~66 FPS para animación fluida
            timerAnimacion.Tick += TimerAnimacion_Tick;
        }

        /// <summary>
        /// Evento de carga del formulario.
        /// Inicializa todos los componentes visuales y configura los eventos de validación.
        /// </summary>
        private void FrmRegistro_Load(object sender, EventArgs e)
        {
            CargarLogo();
            CentrarContenedor();
            LimpiarErrores();
            ConfigurarEventosCampos();
            txtNombre.Focus();
        }

        /// <summary>
        /// Carga el logo de NexumApp desde la carpeta Resources.
        /// </summary>
        private void CargarLogo()
        {
            try
            {
                string rutaLogo = Path.Combine(Application.StartupPath, "Resources", "logo.png");
                if (File.Exists(rutaLogo))
                {
                    picLogo.Image = Image.FromFile(rutaLogo);
                }
            }
            catch { /* Si no existe el logo, el PictureBox quedará vacío */ }
        }

        /// <summary>
        /// Centra el panel contenedor principal en la ventana.
        /// Se llama al cargar y al redimensionar el formulario.
        /// </summary>
        private void CentrarContenedor()
        {
            int x = (this.ClientSize.Width - pnlContenedor.Width) / 2;
            int y = (this.ClientSize.Height - pnlContenedor.Height) / 2;
            pnlContenedor.Location = new Point(Math.Max(0, x), Math.Max(0, y));
        }

        /// <summary>
        /// Recentra el contenedor cuando se redimensiona la ventana.
        /// </summary>
        private void FrmRegistro_Resize(object sender, EventArgs e)
        {
            CentrarContenedor();
        }

        /// <summary>
        /// Configura los eventos de foco y cambio de texto para todos los campos.
        /// Esto permite el feedback visual en tiempo real.
        /// </summary>
        private void ConfigurarEventosCampos()
        {
            // Configura cada campo con sus paneles y labels de error correspondientes
            ConfigurarCampo(txtNombre, pnlNombreContainer, lblErrorNombre);
            ConfigurarCampo(txtApellidos, pnlApellidosContainer, lblErrorApellidos);
            ConfigurarCampo(txtEmail, pnlEmailContainer, lblErrorEmail);
            ConfigurarCampo(txtPassword, pnlPasswordContainer, lblErrorPassword);
            ConfigurarCampo(txtConfirmar, pnlConfirmarContainer, lblErrorConfirmar);

            // Eventos especiales para validación de contraseña
            txtPassword.TextChanged += ValidarFortalezaPassword;
            txtConfirmar.TextChanged += ValidarCoincidenciaPassword;
        }

        /// <summary>
        /// Configura los eventos visuales de un campo de texto.
        /// </summary>
        /// <param name="txt">TextBox a configurar.</param>
        /// <param name="pnl">Panel contenedor del campo (para cambiar color de fondo).</param>
        /// <param name="lblError">Label donde se mostrará el error.</param>
        private void ConfigurarCampo(TextBox txt, Panel pnl, Label lblError)
        {
            // Cambiar a color de foco al entrar
            txt.GotFocus += (s, e) =>
            {
                if (pnl.BackColor != colorFondoCampoError)
                    pnl.BackColor = colorFondoCampoFocus;
            };

            // Restaurar color al salir (si no hay error ni éxito)
            txt.LostFocus += (s, e) =>
            {
                if (pnl.BackColor != colorFondoCampoError && pnl.BackColor != colorFondoCampoExito)
                    pnl.BackColor = colorFondoCampo;
            };

            // Limpiar error cuando el usuario empieza a corregir
            txt.TextChanged += (s, e) =>
            {
                if (pnl.BackColor == colorFondoCampoError)
                {
                    pnl.BackColor = txt.Focused ? colorFondoCampoFocus : colorFondoCampo;
                    lblError.Text = "";
                }
            };
        }

        /// <summary>
        /// Evalúa la fortaleza de la contraseña en tiempo real.
        /// Muestra un indicador visual con el nivel de seguridad.
        /// </summary>
        /// <remarks>
        /// Criterios de fortaleza (cada uno suma 1 punto):
        /// - Longitud >= 8 caracteres
        /// - Contiene mayúsculas
        /// - Contiene minúsculas
        /// - Contiene números
        /// - Contiene caracteres especiales
        /// 
        /// Niveles: Débil (0-2), Media (3), Segura (4-5)
        /// </remarks>
        private void ValidarFortalezaPassword(object sender, EventArgs e)
        {
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(password))
            {
                lblErrorPassword.Text = "";
                lblErrorPassword.ForeColor = Color.FromArgb(255, 100, 100);
                return;
            }

            // Calcular puntuación de fortaleza
            int fortaleza = 0;
            if (password.Length >= 8) fortaleza++;
            if (Regex.IsMatch(password, @"[A-Z]")) fortaleza++;
            if (Regex.IsMatch(password, @"[a-z]")) fortaleza++;
            if (Regex.IsMatch(password, @"[0-9]")) fortaleza++;
            if (Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]")) fortaleza++;

            // Mostrar indicador según nivel
            if (fortaleza <= 2)
            {
                lblErrorPassword.Text = "⚠ Contraseña débil";
                lblErrorPassword.ForeColor = Color.FromArgb(255, 150, 100);
            }
            else if (fortaleza <= 3)
            {
                lblErrorPassword.Text = "◐ Contraseña media";
                lblErrorPassword.ForeColor = Color.FromArgb(255, 200, 100);
            }
            else
            {
                lblErrorPassword.Text = "✓ Contraseña segura";
                lblErrorPassword.ForeColor = Color.FromArgb(100, 200, 100);
            }
        }

        /// <summary>
        /// Verifica en tiempo real si las contraseñas coinciden.
        /// Actualiza el indicador visual según el resultado.
        /// </summary>
        private void ValidarCoincidenciaPassword(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtConfirmar.Text))
            {
                lblErrorConfirmar.Text = "";
                pnlConfirmarContainer.BackColor = txtConfirmar.Focused ? colorFondoCampoFocus : colorFondoCampo;
                return;
            }

            if (txtPassword.Text == txtConfirmar.Text)
            {
                lblErrorConfirmar.Text = "✓ Las contraseñas coinciden";
                lblErrorConfirmar.ForeColor = Color.FromArgb(100, 200, 100);
                pnlConfirmarContainer.BackColor = colorFondoCampoExito;
            }
            else
            {
                lblErrorConfirmar.Text = "✕ Las contraseñas no coinciden";
                lblErrorConfirmar.ForeColor = Color.FromArgb(255, 100, 100);
                pnlConfirmarContainer.BackColor = colorFondoCampoError;
            }
        }

        #region Sistema de Notificaciones

        /// <summary>
        /// Tipos de notificación disponibles, cada uno con un color distintivo.
        /// </summary>
        private enum TipoNotificacion
        {
            /// <summary>Error crítico (rojo).</summary>
            Error,
            /// <summary>Advertencia (amarillo).</summary>
            Warning,
            /// <summary>Información neutral (azul).</summary>
            Info,
            /// <summary>Operación exitosa (verde).</summary>
            Exito
        }

        /// <summary>
        /// Muestra una notificación animada tipo "toast" en la parte superior del formulario.
        /// </summary>
        /// <param name="mensaje">Texto a mostrar en la notificación.</param>
        /// <param name="tipo">Tipo de notificación que determina el color.</param>
        private void MostrarNotificacion(string mensaje, TipoNotificacion tipo = TipoNotificacion.Error)
        {
            Color colorFondo;
            Color colorTexto = Color.White;

            switch (tipo)
            {
                case TipoNotificacion.Warning:
                    colorFondo = colorNotificacionWarning;
                    colorTexto = Color.FromArgb(30, 30, 30);
                    break;
                case TipoNotificacion.Info:
                    colorFondo = colorNotificacionInfo;
                    break;
                case TipoNotificacion.Exito:
                    colorFondo = colorNotificacionExito;
                    break;
                default:
                    colorFondo = colorNotificacionError;
                    break;
            }

            pnlNotificacion.BackColor = colorFondo;
            lblNotificacion.ForeColor = colorTexto;
            lblNotificacion.Text = mensaje;

            pnlNotificacion.Visible = true;
            alturaObjetivo = ALTURA_NOTIFICACION;
            timerAnimacion.Start();

            timerNotificacion.Stop();
            timerNotificacion.Start();
        }

        /// <summary>
        /// Oculta la notificación con animación de colapso.
        /// </summary>
        private void OcultarNotificacion()
        {
            alturaObjetivo = 0;
            timerAnimacion.Start();
        }

        /// <summary>
        /// Controla la animación suave del panel de notificación.
        /// </summary>
        private void TimerAnimacion_Tick(object sender, EventArgs e)
        {
            int diferencia = alturaObjetivo - pnlNotificacion.Height;

            if (Math.Abs(diferencia) <= 3)
            {
                pnlNotificacion.Height = alturaObjetivo;
                timerAnimacion.Stop();

                if (alturaObjetivo == 0)
                    pnlNotificacion.Visible = false;
            }
            else
            {
                pnlNotificacion.Height += diferencia / 4 + (diferencia > 0 ? 1 : -1);
            }
        }

        /// <summary>
        /// Se ejecuta cuando expira el tiempo de visualización de la notificación.
        /// </summary>
        private void TimerNotificacion_Tick(object sender, EventArgs e)
        {
            timerNotificacion.Stop();
            OcultarNotificacion();
        }

        /// <summary>
        /// Muestra un mensaje de error debajo de un campo específico.
        /// </summary>
        /// <param name="contenedor">Panel contenedor del campo.</param>
        /// <param name="lblError">Label donde mostrar el error.</param>
        /// <param name="mensaje">Texto del error.</param>
        private void MostrarErrorCampo(Panel contenedor, Label lblError, string mensaje)
        {
            contenedor.BackColor = colorFondoCampoError;
            lblError.Text = mensaje;
            lblError.ForeColor = Color.FromArgb(255, 100, 100);
        }

        /// <summary>
        /// Limpia todos los errores del formulario y restaura los colores por defecto.
        /// </summary>
        private void LimpiarErrores()
        {
            lblErrorNombre.Text = "";
            lblErrorApellidos.Text = "";
            lblErrorEmail.Text = "";
            lblErrorPassword.Text = "";
            lblErrorConfirmar.Text = "";

            pnlNombreContainer.BackColor = colorFondoCampo;
            pnlApellidosContainer.BackColor = colorFondoCampo;
            pnlEmailContainer.BackColor = colorFondoCampo;
            pnlPasswordContainer.BackColor = colorFondoCampo;
            pnlConfirmarContainer.BackColor = colorFondoCampo;
        }

        #endregion

        /// <summary>
        /// Manejador del botón de registro.
        /// Valida todos los campos, verifica duplicados y crea el usuario.
        /// </summary>
        /// <remarks>
        /// Proceso de registro:
        /// 1. Validar todos los campos (vacíos, formatos, coincidencias)
        /// 2. Verificar que el email no esté ya registrado
        /// 3. Llamar a AuthService.RegistrarUsuario con la contraseña (se hashea internamente)
        /// 4. Si es exitoso, cerrar el formulario y volver al login
        /// </remarks>
        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            OcultarNotificacion();

            // Obtener valores de los campos
            string nombre = txtNombre.Text.Trim();
            string apellidos = txtApellidos.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string confirmar = txtConfirmar.Text;

            bool hayErrores = false;

            // Validación del nombre
            if (string.IsNullOrEmpty(nombre))
            {
                MostrarErrorCampo(pnlNombreContainer, lblErrorNombre, "El nombre es obligatorio");
                hayErrores = true;
            }
            else if (nombre.Length < 2)
            {
                MostrarErrorCampo(pnlNombreContainer, lblErrorNombre, "Nombre demasiado corto");
                hayErrores = true;
            }

            // Validación de apellidos
            if (string.IsNullOrEmpty(apellidos))
            {
                MostrarErrorCampo(pnlApellidosContainer, lblErrorApellidos, "Los apellidos son obligatorios");
                hayErrores = true;
            }

            // Validación de email con expresión regular
            if (string.IsNullOrEmpty(email))
            {
                MostrarErrorCampo(pnlEmailContainer, lblErrorEmail, "El email es obligatorio");
                hayErrores = true;
            }
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MostrarErrorCampo(pnlEmailContainer, lblErrorEmail, "El formato del email no es válido");
                hayErrores = true;
            }

            // Validación de contraseña
            if (string.IsNullOrEmpty(password))
            {
                MostrarErrorCampo(pnlPasswordContainer, lblErrorPassword, "La contraseña es obligatoria");
                hayErrores = true;
            }
            else if (password.Length < 6)
            {
                MostrarErrorCampo(pnlPasswordContainer, lblErrorPassword, "Mínimo 6 caracteres");
                hayErrores = true;
            }

            // Validación de confirmación de contraseña
            if (string.IsNullOrEmpty(confirmar))
            {
                MostrarErrorCampo(pnlConfirmarContainer, lblErrorConfirmar, "Confirma tu contraseña");
                hayErrores = true;
            }
            else if (password != confirmar)
            {
                MostrarErrorCampo(pnlConfirmarContainer, lblErrorConfirmar, "Las contraseñas no coinciden");
                hayErrores = true;
            }

            if (hayErrores)
            {
                MostrarNotificacion("Revisa los campos marcados en rojo", TipoNotificacion.Warning);
                return;
            }

            // Verificar si el email ya está registrado antes de intentar crear
            if (_authService.EmailExiste(email))
            {
                MostrarErrorCampo(pnlEmailContainer, lblErrorEmail, "Este email ya está registrado");
                MostrarNotificacion("Ya existe una cuenta con este email", TipoNotificacion.Error);
                txtEmail.Focus();
                return;
            }

            // Feedback visual: deshabilitar botón mientras se procesa
            btnRegistrar.Enabled = false;
            btnRegistrar.Text = "CREANDO CUENTA...";

            try
            {
                // Llamar al servicio para crear el usuario (la contraseña se hashea internamente)
                bool registrado = _authService.RegistrarUsuario(nombre, apellidos, email, password);

                if (registrado)
                {
                    MostrarNotificacion("✓ ¡Cuenta creada exitosamente!", TipoNotificacion.Exito);

                    // Timer para dar tiempo a que el usuario vea el mensaje de éxito
                    Timer timerCerrar = new Timer();
                    timerCerrar.Interval = 1500;
                    timerCerrar.Tick += (s, ev) =>
                    {
                        timerCerrar.Stop();
                        this.DialogResult = DialogResult.OK; // Indica al login que el registro fue exitoso
                        this.Close();
                    };
                    timerCerrar.Start();
                }
                else
                {
                    MostrarNotificacion("Error al crear la cuenta. Intenta de nuevo.", TipoNotificacion.Error);
                    btnRegistrar.Enabled = true;
                    btnRegistrar.Text = "CREAR CUENTA";
                }
            }
            catch (Exception ex)
            {
                MostrarNotificacion($"Error: {ex.Message}", TipoNotificacion.Error);
                btnRegistrar.Enabled = true;
                btnRegistrar.Text = "CREAR CUENTA";
            }
        }

        /// <summary>
        /// Cierra el formulario de registro y vuelve al login.
        /// </summary>
        private void LnkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Cierra el formulario de registro.
        /// </summary>
        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
