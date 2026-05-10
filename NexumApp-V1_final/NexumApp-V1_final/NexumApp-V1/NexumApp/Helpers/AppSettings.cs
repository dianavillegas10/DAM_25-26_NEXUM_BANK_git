using NexumApp.Models;
using System;
using System.Drawing;
using System.Globalization;

namespace NexumApp.Helpers
{
    /// <summary>
    /// Configuración global de la app en tiempo real.
    /// Se actualiza al guardar configuración y es leída por todos los formularios.
    /// </summary>
    public static class AppSettings
    {
        // ── Tema ──────────────────────────────────────────────
        public static bool ModoOscuro     { get; private set; } = true;
        public static bool AltoContraste  { get; private set; } = false;
        public static int  TamanoFuente   { get; private set; } = 100;

        // ── Preferencias ──────────────────────────────────────
        public static bool   MostrarSaldoInicio      { get; private set; } = true;
        public static bool   OrdenarCuentasPorSaldo  { get; private set; } = false;
        public static bool   ConfirmarTransferencias { get; private set; } = true;
        public static bool   GuardarBeneficiarios    { get; private set; } = true;

        // ── Notificaciones ────────────────────────────────────
        public static bool NotificacionesEmail { get; private set; } = true;
        public static bool NotificacionesPush  { get; private set; } = true;

        // ── Seguridad / sesión ────────────────────────────────
        public static int     TiempoSesionMinutos  { get; private set; } = 30;
        public static string  MonedaPreferida       { get; private set; } = "EUR";
        public static decimal PresupuestoObjetivo   { get; set; }        = 0m;

        // ── Colores del tema activo ────────────────────────────
        public static Color BgPrincipal   { get; private set; }
        public static Color BgCard        { get; private set; }
        public static Color BgInput       { get; private set; }
        public static Color TextoPrimario { get; private set; }
        public static Color TextoMuted    { get; private set; }
        public static Color BorderColor   { get; private set; }

        // ── Evento global: cualquier vista puede suscribirse ──
        public static event EventHandler ConfiguracionChanged;

        static AppSettings() => AplicarTema(true);

        // ─────────────────────────────────────────────────────
        //  CARGA COMPLETA DESDE CONFIGURACIÓN DE USUARIO
        // ─────────────────────────────────────────────────────
        public static void CargarDesdeConfiguracion(ConfiguracionUsuario cfg)
        {
            if (cfg == null) return;

            MostrarSaldoInicio      = cfg.MostrarSaldoInicio;
            OrdenarCuentasPorSaldo  = cfg.OrdenarCuentasPorSaldo;
            ConfirmarTransferencias = cfg.ConfirmarTransferencias;
            GuardarBeneficiarios    = cfg.GuardarBeneficiarios;
            NotificacionesEmail     = cfg.NotificacionesEmail;
            NotificacionesPush      = cfg.NotificacionesPush;
            TiempoSesionMinutos     = cfg.TiempoSesionMinutos;
            MonedaPreferida         = cfg.MonedaPreferida ?? "EUR";
            if (cfg.PresupuestoObjetivo > 0) PresupuestoObjetivo = cfg.PresupuestoObjetivo;
            AltoContraste           = cfg.AltoContraste;
            TamanoFuente            = Math.Max(75, Math.Min(150, cfg.TamanoFuente));

            AplicarIdioma(cfg.Idioma);
            AplicarTema(cfg.ModoOscuro);
        }

        /// <summary>Dispara el evento global para que los formularios abiertos se actualicen.</summary>
        public static void NotificarCambio() =>
            ConfiguracionChanged?.Invoke(null, EventArgs.Empty);

        /// <summary>
        /// Recorre todos los controles de un contenedor y traduce los textos que estén en el diccionario.
        /// Úsalo tras construir cualquier vista para aplicar el idioma activo.
        /// </summary>
        public static void AplicarTraduccionesRecursivo(System.Windows.Forms.Control raiz)
        {
            if (Idioma == "es") return; // español es el idioma base, no hay que cambiar nada
            foreach (System.Windows.Forms.Control c in raiz.Controls)
            {
                if (!string.IsNullOrEmpty(c.Text))
                {
                    string trad = T(c.Text);
                    if (trad != c.Text) c.Text = trad;
                }
                if (c.HasChildren) AplicarTraduccionesRecursivo(c);
            }
        }

        // ─────────────────────────────────────────────────────
        //  TEMA (oscuro / claro / alto contraste)
        // ─────────────────────────────────────────────────────
        public static void AplicarTema(bool oscuro)
        {
            ModoOscuro = oscuro;
            if (oscuro)
            {
                BgPrincipal   = Color.FromArgb(10,  12,  28);
                BgCard        = Color.FromArgb(18,  22,  46);
                BgInput       = Color.FromArgb(24,  29,  58);
                TextoPrimario = Color.FromArgb(241, 245, 249);
                TextoMuted    = Color.FromArgb(100, 116, 139);
                BorderColor   = Color.FromArgb(38,  44,  80);
            }
            else if (AltoContraste)
            {
                BgPrincipal   = Color.FromArgb(255, 255, 255);
                BgCard        = Color.FromArgb(255, 255, 255);
                BgInput       = Color.FromArgb(240, 240, 240);
                TextoPrimario = Color.FromArgb(0,   0,   0);
                TextoMuted    = Color.FromArgb(30,  30,  30);
                BorderColor   = Color.FromArgb(0,   0,   0);
            }
            else
            {
                BgPrincipal   = Color.FromArgb(244, 247, 254);
                BgCard        = Color.White;
                BgInput       = Color.FromArgb(245, 247, 250);
                TextoPrimario = Color.FromArgb(31,  41,  55);
                TextoMuted    = Color.FromArgb(107, 114, 128);
                BorderColor   = Color.FromArgb(220, 225, 235);
            }
        }

        public static void AplicarAltoContraste(bool activo)
        {
            AltoContraste = activo;
            AplicarTema(ModoOscuro);
        }

        public static void AplicarFuente(int porcentaje)
        {
            TamanoFuente = Math.Max(75, Math.Min(150, porcentaje));
        }

        /// <summary>Escala un tamaño de fuente base según TamanoFuente.</summary>
        public static float EscalarFuente(float baseSize) =>
            baseSize * TamanoFuente / 100f;

        // ─────────────────────────────────────────────────────
        //  IDIOMA
        // ─────────────────────────────────────────────────────
        public static string Idioma { get; private set; } = "es";

        public static void AplicarIdioma(string codigo) => Idioma = codigo ?? "es";

        /// <summary>Traduce un texto clave al idioma activo.</summary>
        public static string T(string clave)
        {
            switch (Idioma)
            {
                case "en": return EN(clave);
                case "ca": return CA(clave);
                default:   return clave;
            }
        }

        // ── Traducciones Inglés ───────────────────────────────
        private static string EN(string k)
        {
            switch (k)
            {
                // Sidebar secciones y navegación
                case "PRINCIPAL":               return "MAIN";
                case "FINANZAS":                return "FINANCE";
                case "Inicio":                  return "Home";
                case "Cuentas":                 return "Accounts";
                case "Transferencias":          return "Transfers";
                case "Pagos y Recargas":        return "Payments";
                case "Inversiones":             return "Investments";
                case "Tarjetas":                return "Cards";
                case "Préstamos":               return "Loans";
                case "Ayuda / Tickets":         return "Help / Tickets";
                case "Configuración":           return "Settings";
                case "Cerrar Sesión":           return "Log Out";
                case "Cerrar sesión":           return "Log out";
                // Header
                case "Bienvenido a Nexum Bank": return "Welcome to Nexum Bank";
                case "Mi perfil":               return "My profile";
                case "Editar":                  return "Edit";
                // Dashboard — saldos y movimientos
                case "Saldo disponible":        return "Available balance";
                case "Movimientos Recientes":   return "Recent Transactions";
                case "Últimos movimientos":     return "Recent Movements";
                case "Ver todos":               return "View all";
                case "Ver todos >":             return "View all >";
                case "Ver todo":                return "View all";
                case "Sin movimientos registrados.": return "No transactions recorded.";
                // Dashboard — cuentas y tarjetas
                case "Mis Tarjetas":            return "My Cards";
                case "Mis Cuentas":             return "My Accounts";
                case "Abrir nueva cuenta":      return "Open new account";
                // Dashboard — acciones rápidas
                case "Enviar dinero":           return "Send money";
                case "Recibir dinero":          return "Receive money";
                case "Transferir":              return "Transfer";
                case "Pagar servicios":         return "Pay services";
                case "Recargar móvil":          return "Top up mobile";
                case "Invertir":                return "Invest";
                case "Analizar gastos":         return "Analyse spending";
                case "Dividir cuenta":          return "Split bill";
                case "Nueva transferencia":     return "New Transfer";
                case "Ingresar efectivo":       return "Deposit Cash";
                case "Retirar efectivo":        return "Withdraw Cash";
                // Dashboard — secciones
                case "Novedades":               return "What's new";
                case "Becas del Banco":         return "Bank Grants";
                case "Plan de Pensiones":       return "Pension Plan";
                case "Mis Huchas":              return "My Piggy Banks";
                case "Objetivos de Ahorro":     return "Savings Goals";
                case "Acciones Rápidas":        return "Quick Actions";
                case "Presupuesto del mes":     return "Monthly Budget";
                case "Ingresos":                return "Income";
                case "Gastos":                  return "Expenses";
                case "Beneficios Nexum":        return "Nexum Benefits";
                case "Puntos acumulados":       return "Accumulated points";
                case "Cashback":                return "Cashback";
                // Configuración
                case "Apariencia":              return "Appearance";
                case "Notificaciones":          return "Notifications";
                case "Seguridad":               return "Security";
                case "Preferencias":            return "Preferences";
                case "Mi cuenta":               return "My account";
                case "Guardar cambios":         return "Save changes";
                case "Restablecer":             return "Reset";
                case "Modo oscuro":             return "Dark mode";
                case "Alto contraste":          return "High contrast";
                case "Tamaño de texto":         return "Text size";
                case "Idioma y región":         return "Language & region";
                case "Cambiar contraseña":      return "Change password";
                // Cuentas
                case "SALDO TOTAL":             return "TOTAL BALANCE";
                case "Saldo total":             return "Total balance";
                case "SALDO DISPONIBLE":        return "AVAILABLE BALANCE";
                case "INGRESOS MES":            return "MONTHLY INCOME";
                case "GASTOS MES":              return "MONTHLY EXPENSES";
                case "APERTURA":                return "OPENED";
                case "Abierta el":              return "Opened on";
                case "Aún no tienes cuentas.\nPulsa el botón para abrir tu primera cuenta.":
                                                return "You have no accounts yet.\nTap the button to open your first account.";
                case "Aún no tienes cuentas.\nPulsa el botón para crear una.":
                                                return "You have no accounts yet.\nTap the button to create one.";
                // Transferencias
                case "Historial de transferencias":     return "Transfer history";
                case "Tus últimas 20 operaciones enviadas": return "Your last 20 outgoing transfers";
                case "DESDE TU CUENTA":         return "FROM YOUR ACCOUNT";
                case "NOMBRE DEL BENEFICIARIO": return "BENEFICIARY NAME";
                case "IBAN / CUENTA DESTINO":   return "IBAN / DESTINATION ACCOUNT";
                case "IMPORTE A ENVIAR":        return "AMOUNT TO SEND";
                case "ACCESO RÁPIDO":           return "QUICK AMOUNT";
                case "CONCEPTO":                return "REFERENCE";
                case "Resumen":                 return "Summary";
                case "Estadísticas de tus envíos": return "Your transfer stats";
                case "ESTE MES":                return "THIS MONTH";
                case "HOY":                     return "TODAY";
                case "TOTAL HISTÓRICO":         return "ALL TIME";
                case "COMPLETADAS":             return "COMPLETED";
                case "DESTINOS RECIENTES":      return "RECENT DESTINATIONS";
                case "Continuar →":             return "Continue →";
                case "← Volver y editar":       return "← Back and edit";
                // Pagos
                case "Paga tus facturas de forma rápida y segura": return "Pay your bills quickly and securely";
                case "CUENTA DE CARGO":         return "PAYMENT ACCOUNT";
                case "REFERENCIA / NÚMERO DE CONTRATO": return "REFERENCE / CONTRACT NUMBER";
                case "IMPORTE":                 return "AMOUNT";
                case "Electricidad":            return "Electricity";
                case "Internet":                return "Internet";
                case "Seguro Hogar":            return "Home Insurance";
                case "Comunidad":               return "Community fees";
                // Inversiones
                case "Invertir ahora":          return "Invest now";
                case "Fondos disponibles":      return "Available funds";
                case "Rentabilidad":            return "Returns";
                case "Riesgo":                  return "Risk";
                case "Bajo":                    return "Low";
                case "Medio":                   return "Medium";
                case "Alto":                    return "High";
                case "Plazo":                   return "Term";
                // Tarjetas
                case "Bloquear tarjeta":        return "Block card";
                case "Desbloquear tarjeta":     return "Unblock card";
                case "Límite diario":           return "Daily limit";
                case "Límite mensual":          return "Monthly limit";
                case "Solicitar nueva tarjeta": return "Request new card";
                case "Número de tarjeta":       return "Card number";
                case "Fecha de vencimiento":    return "Expiry date";
                case "Tarjeta principal":       return "Main card";
                // Préstamos
                case "Solicitar préstamo":      return "Apply for loan";
                case "Cuota mensual":           return "Monthly payment";
                case "Capital pendiente":       return "Outstanding balance";
                case "Tipo de interés":         return "Interest rate";
                case "Simulador":               return "Simulator";
                case "Simular préstamo":        return "Simulate loan";
                case "Ver detalle":             return "View details";
                case "Préstamos activos":       return "Active loans";
                case "Sin préstamos activos":   return "No active loans";
                // Ayuda / Tickets
                case "Crear ticket":            return "Create ticket";
                case "Mis tickets":             return "My tickets";
                case "Estado":                  return "Status";
                case "Abierto":                 return "Open";
                case "Cerrado":                 return "Closed";
                case "En proceso":              return "In progress";
                case "Soporte técnico":         return "Technical support";
                case "Preguntas frecuentes":    return "FAQ";
                case "Centro de ayuda":         return "Help center";
                case "Enviar consulta":         return "Send query";
                // Configuración
                case "Activar":                 return "Enable";
                case "Desactivar":              return "Disable";
                case "Configurar PIN 2FA":      return "Set up 2FA PIN";
                case "Verificación en dos pasos": return "Two-step verification";
                case "Correo electrónico":      return "Email";
                case "Notificaciones push":     return "Push notifications";
                case "Tiempo de sesión":        return "Session timeout";
                case "Moneda preferida":        return "Preferred currency";
                case "Mostrar saldo al inicio": return "Show balance on home";
                case "Confirmar transferencias": return "Confirm transfers";
                default:                        return k;
            }
        }

        // ── Traducciones Catalán ──────────────────────────────
        private static string CA(string k)
        {
            switch (k)
            {
                // Sidebar secciones y navegación
                case "PRINCIPAL":               return "PRINCIPAL";
                case "FINANZAS":                return "FINANCES";
                case "Inicio":                  return "Inici";
                case "Cuentas":                 return "Comptes";
                case "Transferencias":          return "Transferències";
                case "Pagos y Recargas":        return "Pagaments";
                case "Inversiones":             return "Inversions";
                case "Tarjetas":                return "Targetes";
                case "Préstamos":               return "Préstecs";
                case "Ayuda / Tickets":         return "Ajuda / Tiquets";
                case "Configuración":           return "Configuració";
                case "Cerrar Sesión":           return "Tancar sessió";
                case "Cerrar sesión":           return "Tancar sessió";
                // Header
                case "Bienvenido a Nexum Bank": return "Benvingut a Nexum Bank";
                case "Mi perfil":               return "El meu perfil";
                case "Editar":                  return "Editar";
                // Dashboard — saldos y movimientos
                case "Saldo disponible":        return "Saldo disponible";
                case "Movimientos Recientes":   return "Moviments recents";
                case "Últimos movimientos":     return "Darrers moviments";
                case "Ver todos":               return "Veure tots";
                case "Ver todos >":             return "Veure tots >";
                case "Ver todo":                return "Veure tot";
                case "Sin movimientos registrados.": return "Sense moviments registrats.";
                // Dashboard — cuentas y tarjetas
                case "Mis Tarjetas":            return "Les meves targetes";
                case "Mis Cuentas":             return "Els meus comptes";
                case "Abrir nueva cuenta":      return "Obrir compte nou";
                // Dashboard — acciones rápidas
                case "Enviar dinero":           return "Enviar diners";
                case "Recibir dinero":          return "Rebre diners";
                case "Transferir":              return "Transferir";
                case "Pagar servicios":         return "Pagar serveis";
                case "Recargar móvil":          return "Recarregar mòbil";
                case "Invertir":                return "Invertir";
                case "Analizar gastos":         return "Analitzar despeses";
                case "Dividir cuenta":          return "Dividir compte";
                case "Nueva transferencia":     return "Nova transferència";
                case "Ingresar efectivo":       return "Ingressar efectiu";
                case "Retirar efectivo":        return "Retirar efectiu";
                // Dashboard — secciones
                case "Novedades":               return "Novetats";
                case "Becas del Banco":         return "Beques del Banc";
                case "Plan de Pensiones":       return "Pla de Pensions";
                case "Mis Huchas":              return "Les meves guardioles";
                case "Objetivos de Ahorro":     return "Objectius d'estalvi";
                case "Acciones Rápidas":        return "Accions Ràpides";
                case "Presupuesto del mes":     return "Pressupost del mes";
                case "Ingresos":                return "Ingressos";
                case "Gastos":                  return "Despeses";
                // Configuración
                case "Apariencia":              return "Aparença";
                case "Notificaciones":          return "Notificacions";
                case "Seguridad":               return "Seguretat";
                case "Preferencias":            return "Preferències";
                case "Mi cuenta":               return "El meu compte";
                case "Guardar cambios":         return "Desar canvis";
                case "Restablecer":             return "Restablir";
                case "Modo oscuro":             return "Mode fosc";
                case "Alto contraste":          return "Alt contrast";
                case "Tamaño de texto":         return "Mida del text";
                case "Idioma y región":         return "Idioma i regió";
                case "Cambiar contraseña":      return "Canviar contrasenya";
                // Cuentas
                case "SALDO TOTAL":             return "SALDO TOTAL";
                case "Saldo total":             return "Saldo total";
                case "SALDO DISPONIBLE":        return "SALDO DISPONIBLE";
                case "INGRESOS MES":            return "INGRESSOS MES";
                case "GASTOS MES":              return "DESPESES MES";
                case "APERTURA":                return "OBERTURA";
                case "Abierta el":              return "Oberta el";
                case "Aún no tienes cuentas.\nPulsa el botón para abrir tu primera cuenta.":
                                                return "Encara no tens comptes.\nPrem el botó per obrir el primer compte.";
                case "Aún no tienes cuentas.\nPulsa el botón para crear una.":
                                                return "Encara no tens comptes.\nPrem el botó per crear-ne un.";
                // Transferencias
                case "Historial de transferencias":     return "Historial de transferències";
                case "Tus últimas 20 operaciones enviadas": return "Les teves darreres 20 operacions enviades";
                case "DESDE TU CUENTA":         return "DES DEL TEU COMPTE";
                case "NOMBRE DEL BENEFICIARIO": return "NOM DEL BENEFICIARI";
                case "IBAN / CUENTA DESTINO":   return "IBAN / COMPTE DESTÍ";
                case "IMPORTE A ENVIAR":        return "IMPORT A ENVIAR";
                case "ACCESO RÁPIDO":           return "ACCÉS RÀPID";
                case "CONCEPTO":                return "CONCEPTE";
                case "Resumen":                 return "Resum";
                case "Estadísticas de tus envíos": return "Estadístiques dels teus enviaments";
                case "ESTE MES":                return "AQUEST MES";
                case "HOY":                     return "AVUI";
                case "TOTAL HISTÓRICO":         return "TOTAL HISTÒRIC";
                case "COMPLETADAS":             return "COMPLETADES";
                case "DESTINOS RECIENTES":      return "DESTINATARIS RECENTS";
                case "Continuar →":             return "Continuar →";
                case "← Volver y editar":       return "← Tornar i editar";
                // Pagos
                case "Paga tus facturas de forma rápida y segura": return "Paga les teves factures de manera ràpida i segura";
                case "CUENTA DE CARGO":         return "COMPTE DE CÀRREC";
                case "REFERENCIA / NÚMERO DE CONTRATO": return "REFERÈNCIA / NÚMERO DE CONTRACTE";
                case "IMPORTE":                 return "IMPORT";
                case "Electricidad":            return "Electricitat";
                case "Internet":                return "Internet";
                case "Seguro Hogar":            return "Assegurança llar";
                case "Comunidad":               return "Comunitat";
                // Inversiones
                case "Invertir ahora":          return "Invertir ara";
                case "Fondos disponibles":      return "Fons disponibles";
                case "Rentabilidad":            return "Rendibilitat";
                case "Riesgo":                  return "Risc";
                case "Bajo":                    return "Baix";
                case "Medio":                   return "Mitjà";
                case "Alto":                    return "Alt";
                case "Plazo":                   return "Termini";
                // Tarjetas
                case "Bloquear tarjeta":        return "Bloquejar targeta";
                case "Desbloquear tarjeta":     return "Desbloquejar targeta";
                case "Límite diario":           return "Límit diari";
                case "Límite mensual":          return "Límit mensual";
                case "Solicitar nueva tarjeta": return "Sol·licitar nova targeta";
                case "Número de tarjeta":       return "Número de targeta";
                case "Fecha de vencimiento":    return "Data de venciment";
                case "Tarjeta principal":       return "Targeta principal";
                // Préstamos
                case "Solicitar préstamo":      return "Sol·licitar préstec";
                case "Cuota mensual":           return "Quota mensual";
                case "Capital pendiente":       return "Capital pendent";
                case "Tipo de interés":         return "Tipus d'interès";
                case "Simulador":               return "Simulador";
                case "Simular préstamo":        return "Simular préstec";
                case "Ver detalle":             return "Veure detall";
                case "Préstamos activos":       return "Préstecs actius";
                case "Sin préstamos activos":   return "Sense préstecs actius";
                // Ajuda / Tiquets
                case "Crear ticket":            return "Crear tiquet";
                case "Mis tickets":             return "Els meus tiquets";
                case "Estado":                  return "Estat";
                case "Abierto":                 return "Obert";
                case "Cerrado":                 return "Tancat";
                case "En proceso":              return "En procés";
                case "Soporte técnico":         return "Suport tècnic";
                case "Preguntas frecuentes":    return "Preguntes freqüents";
                case "Centro de ayuda":         return "Centre d'ajuda";
                case "Enviar consulta":         return "Enviar consulta";
                // Configuració
                case "Activar":                 return "Activar";
                case "Desactivar":              return "Desactivar";
                case "Configurar PIN 2FA":      return "Configurar PIN 2FA";
                case "Verificación en dos pasos": return "Verificació en dos passos";
                case "Correo electrónico":      return "Correu electrònic";
                case "Notificaciones push":     return "Notificacions push";
                case "Tiempo de sesión":        return "Temps de sessió";
                case "Moneda preferida":        return "Moneda preferida";
                case "Mostrar saldo al inicio": return "Mostrar saldo a l'inici";
                case "Confirmar transferencias": return "Confirmar transferències";
                default:                        return k;
            }
        }

        // ─────────────────────────────────────────────────────
        //  MONEDA — CultureInfo y símbolo dinámicos
        // ─────────────────────────────────────────────────────

        /// <summary>
        /// Devuelve el CultureInfo que corresponde a la moneda preferida del usuario.
        /// Úsalo en .ToString("C2", AppSettings.CultureMoneda) en lugar de es-ES fijo.
        /// </summary>
        public static CultureInfo CultureMoneda
        {
            get
            {
                switch (MonedaPreferida)
                {
                    case "USD": return CultureInfo.CreateSpecificCulture("en-US");
                    case "GBP": return CultureInfo.CreateSpecificCulture("en-GB");
                    default:    return CultureInfo.CreateSpecificCulture("es-ES"); // EUR
                }
            }
        }

        /// <summary>Símbolo visual de la moneda preferida (€, $, £).</summary>
        public static string SimboloMoneda
        {
            get
            {
                switch (MonedaPreferida)
                {
                    case "USD": return "$";
                    case "GBP": return "£";
                    default:    return "€";
                }
            }
        }

        private static string _apiBaseUrl;
        public static string ApiBaseUrl
        {
            get => _apiBaseUrl ?? "https://localhost:7001/api/";
            set => _apiBaseUrl = value;
        }
    }
}
