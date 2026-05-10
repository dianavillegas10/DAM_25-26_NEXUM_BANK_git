// Services/TicketService.cs
using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NexumApp.Services
{
    public class TicketService
    {
        private readonly string _ticketsPath;
        private int _nextId = 1;

        public TicketService()
        {
            _ticketsPath = Path.Combine(Application.StartupPath, "Data", "Tickets");
            if (!Directory.Exists(_ticketsPath))
                Directory.CreateDirectory(_ticketsPath);

            CargarUltimoId();
        }

        private void CargarUltimoId()
        {
            try
            {
                var files = Directory.GetFiles(_ticketsPath, "*.json");
                foreach (var file in files)
                {
                    var json = File.ReadAllText(file);
                    var ticket = JsonSerializer.Deserialize<Ticket>(json);
                    if (ticket != null && ticket.Id >= _nextId)
                        _nextId = ticket.Id + 1;
                }
            }
            catch { }
        }

        public async Task<bool> EnviarTicketAsync(TicketRequest request)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var usuario = SesionActual.Instancia?.Usuario;
                    if (usuario == null)
                        return false;

                    var ticket = new Ticket
                    {
                        Id = _nextId++,
                        UsuarioId = usuario.Id,
                        UsuarioNombre = usuario.Nombre,
                        UsuarioEmail = usuario.Email,
                        Asunto = request.Asunto,
                        Descripcion = request.Descripcion,
                        Prioridad = request.Prioridad,
                        Estado = "Pendiente",
                        FechaCreacion = DateTime.Now
                    };

                    string fileName = $"ticket_{ticket.Id:0000}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    string fullPath = Path.Combine(_ticketsPath, fileName);

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(ticket, options);
                    File.WriteAllText(fullPath, json);

                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error guardando ticket: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<List<Ticket>> ObtenerTicketsUsuarioAsync()
        {
            return await Task.Run(() =>
            {
                var tickets = new List<Ticket>();
                try
                {
                    var usuario = SesionActual.Instancia?.Usuario;
                    if (usuario == null) return tickets;

                    var files = Directory.GetFiles(_ticketsPath, "*.json");
                    foreach (var file in files)
                    {
                        var json = File.ReadAllText(file);
                        var ticket = JsonSerializer.Deserialize<Ticket>(json);
                        if (ticket != null && ticket.UsuarioId == usuario.Id)
                            tickets.Add(ticket);
                    }

                    tickets.Sort((a, b) => b.FechaCreacion.CompareTo(a.FechaCreacion));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error cargando tickets: {ex.Message}");
                }
                return tickets;
            });
        }

        /// <summary>
        /// Método de diagnóstico para identificar problemas al cargar tickets.
        /// Muestra mensajes con información detallada sobre el estado del sistema.
        /// </summary>
        public async Task<List<Ticket>> ObtenerTicketsUsuarioConDiagnosticoAsync()
        {
            return await Task.Run(() =>
            {
                var tickets = new List<Ticket>();
                try
                {
                    // Verificar usuario logueado
                    var usuario = SesionActual.Instancia?.Usuario;
                    if (usuario == null)
                    {
                        MessageBox.Show("❌ ERROR: No hay usuario logueado.", "Diagnóstico Tickets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return tickets;
                    }

                    MessageBox.Show($"✅ Usuario logueado: ID={usuario.Id}, Nombre={usuario.Nombre}, Email={usuario.Email}",
                        "Diagnóstico Tickets - Usuario", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Verificar carpeta de tickets
                    if (!Directory.Exists(_ticketsPath))
                    {
                        MessageBox.Show($"❌ ERROR: La carpeta no existe.\nRuta: {_ticketsPath}\n\nCreando carpeta...",
                            "Diagnóstico Tickets - Carpeta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Directory.CreateDirectory(_ticketsPath);
                        return tickets;
                    }

                    MessageBox.Show($"✅ Carpeta encontrada: {_ticketsPath}",
                        "Diagnóstico Tickets - Carpeta", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Buscar archivos
                    var files = Directory.GetFiles(_ticketsPath, "*.json");
                    MessageBox.Show($"📁 Archivos JSON encontrados: {files.Length}\n\nRuta: {_ticketsPath}",
                        "Diagnóstico Tickets - Archivos", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (files.Length == 0)
                    {
                        MessageBox.Show("⚠️ No hay archivos de tickets en la carpeta.\n\nCrea un ticket usando '+ Nuevo Ticket'",
                            "Diagnóstico Tickets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return tickets;
                    }

                    // Leer cada archivo
                    int archivosLeidos = 0;
                    int ticketsEncontrados = 0;
                    int ticketsDelUsuario = 0;

                    foreach (var file in files)
                    {
                        archivosLeidos++;
                        try
                        {
                            var json = File.ReadAllText(file);
                            var ticket = JsonSerializer.Deserialize<Ticket>(json);

                            if (ticket != null)
                            {
                                ticketsEncontrados++;
                                if (ticket.UsuarioId == usuario.Id)
                                {
                                    ticketsDelUsuario++;
                                    tickets.Add(ticket);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error leyendo archivo {file}: {ex.Message}");
                        }
                    }

                    string mensaje = $"📊 RESULTADOS DEL DIAGNÓSTICO:\n\n" +
                                    $"📁 Archivos procesados: {archivosLeidos}\n" +
                                    $"🎫 Tickets encontrados: {ticketsEncontrados}\n" +
                                    $"👤 Tickets de {usuario.Nombre}: {ticketsDelUsuario}\n\n" +
                                    $"📁 Ruta: {_ticketsPath}";

                    if (ticketsDelUsuario == 0)
                    {
                        mensaje += "\n\n⚠️ No se encontraron tickets para este usuario.\n" +
                                  "Posibles causas:\n" +
                                  "• No has creado ningún ticket aún\n" +
                                  "• Los tickets pertenecen a otro usuario\n" +
                                  "• Error al guardar el ID de usuario";
                    }
                    else
                    {
                        mensaje += $"\n\n✅ Se cargaron {ticketsDelUsuario} tickets correctamente.";
                    }

                    MessageBox.Show(mensaje, "Diagnóstico Tickets - Resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    tickets.Sort((a, b) => b.FechaCreacion.CompareTo(a.FechaCreacion));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ ERROR GENERAL: {ex.Message}\n\n{ex.StackTrace}",
                        "Diagnóstico Tickets - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return tickets;
            });
        }

        public async Task<List<Ticket>> ObtenerTodosLosTicketsAdminAsync()
        {
            return await Task.Run(() =>
            {
                var tickets = new List<Ticket>();
                try
                {
                    if (!Directory.Exists(_ticketsPath)) return tickets;

                    var files = Directory.GetFiles(_ticketsPath, "*.json");
                    foreach (var file in files)
                    {
                        var json = File.ReadAllText(file);
                        var ticket = JsonSerializer.Deserialize<Ticket>(json);
                        if (ticket != null)
                        {
                            // Guardamos la ruta del archivo en una propiedad (opcional) 
                            // para saber cuál sobreescribir al responder.
                            tickets.Add(ticket);
                        }
                    }
                    // Ordenar por fecha: los más nuevos arriba
                    tickets.Sort((a, b) => b.FechaCreacion.CompareTo(a.FechaCreacion));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error Admin cargando tickets: {ex.Message}");
                }
                return tickets;
            });
        }

        public void ResponderTicket(int ticketId, string respuesta, string nuevoEstado)
        {
            try
            {
                // Buscamos el archivo que empieza por ese ID
                var file = Directory.GetFiles(_ticketsPath, $"ticket_{ticketId:0000}_*.json").FirstOrDefault();

                if (file != null)
                {
                    var json = File.ReadAllText(file);
                    var ticket = JsonSerializer.Deserialize<Ticket>(json);

                    // Actualizamos los datos
                    ticket.Estado = nuevoEstado;
                    ticket.RespuestaAdmin = respuesta; // Asegúrate de tener esta propiedad en tu clase Ticket.cs
                    ticket.FechaRespuesta = DateTime.Now;

                    // Guardamos los cambios sobreescribiendo el archivo
                    string nuevoJson = JsonSerializer.Serialize(ticket, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(file, nuevoJson);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo guardar la respuesta: " + ex.Message);
            }
        }

        /// <summary>
        /// Método para verificar la sesión actual
        /// </summary>
        public async Task<bool> VerificarSesionAsync()
        {
            return await Task.Run(() =>
            {
                var usuario = SesionActual.Instancia?.Usuario;
                if (usuario == null)
                {
                    MessageBox.Show("❌ No hay sesión activa", "Verificar Sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                MessageBox.Show($"✅ Sesión activa\n\nID: {usuario.Id}\nNombre: {usuario.Nombre}\nEmail: {usuario.Email}\nEsAdmin: {usuario.EsAdmin}",
                    "Verificar Sesión", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            });
        }
    }
}