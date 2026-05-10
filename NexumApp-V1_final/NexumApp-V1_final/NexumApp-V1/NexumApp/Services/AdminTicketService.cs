// Services/AdminTicketService.cs
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
    public class AdminTicketService
    {
        private readonly string _ticketsPath;

        public AdminTicketService()
        {
            _ticketsPath = Path.Combine(Application.StartupPath, "Data", "Tickets");

            if (!Directory.Exists(_ticketsPath))
                Directory.CreateDirectory(_ticketsPath);
        }

        public async Task<List<Ticket>> ObtenerTodosLosTicketsAsync()
        {
            return await Task.Run(() =>
            {
                var tickets = new List<Ticket>();
                try
                {
                    var files = Directory.GetFiles(_ticketsPath, "*.json");
                    foreach (var file in files)
                    {
                        var json = File.ReadAllText(file);
                        var ticket = JsonSerializer.Deserialize<Ticket>(json);
                        if (ticket != null)
                            tickets.Add(ticket);
                    }

                    return tickets.OrderByDescending(t => t.FechaCreacion).ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error cargando tickets: {ex.Message}");
                    return tickets;
                }
            });
        }

        public async Task<bool> ActualizarTicketAsync(Ticket ticket)
        {
            return await Task.Run(() =>
            {
                try
                {
                    ticket.FechaActualizacion = DateTime.Now;

                    // Buscar el archivo original del ticket
                    var files = Directory.GetFiles(_ticketsPath, "*.json");
                    string targetFile = null;

                    foreach (var file in files)
                    {
                        var json = File.ReadAllText(file);
                        var existingTicket = JsonSerializer.Deserialize<Ticket>(json);
                        if (existingTicket != null && existingTicket.Id == ticket.Id)
                        {
                            targetFile = file;
                            break;
                        }
                    }

                    if (targetFile != null)
                    {
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string json = JsonSerializer.Serialize(ticket, options);
                        File.WriteAllText(targetFile, json);
                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error actualizando ticket: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task<Dictionary<string, int>> ObtenerEstadisticasAsync()
        {
            return await Task.Run(async () =>
            {
                var stats = new Dictionary<string, int>();
                try
                {
                    var tickets = await ObtenerTodosLosTicketsAsync();

                    stats["Total"] = tickets.Count;
                    stats["Pendiente"] = tickets.Count(t => t.Estado == "Pendiente");
                    stats["En proceso"] = tickets.Count(t => t.Estado == "En proceso");
                    stats["Resuelto"] = tickets.Count(t => t.Estado == "Resuelto");
                    stats["Cerrado"] = tickets.Count(t => t.Estado == "Cerrado");
                    stats["Urgente"] = tickets.Count(t => t.Prioridad == "Urgente");
                    stats["Alta"] = tickets.Count(t => t.Prioridad == "Alta");
                    stats["Media"] = tickets.Count(t => t.Prioridad == "Media");
                    stats["Baja"] = tickets.Count(t => t.Prioridad == "Baja");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en estadísticas: {ex.Message}");
                }
                return stats;
            });
        }
    }
}