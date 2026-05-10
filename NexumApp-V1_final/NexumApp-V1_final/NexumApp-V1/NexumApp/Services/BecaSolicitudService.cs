using MySql.Data.MySqlClient;
using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace NexumApp.Services
{
    public class BecaSolicitudService
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["NexumDB"].ConnectionString;

        public (bool Exito, string NumeroContrato, string Error) Solicitar(
            int usuarioId, Beca beca, string motivacion,
            string centroEducativo = "", string titulacion = "",
            string anioAcademico = "", string notaMediaODescripcion = "")
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    const string sqlInsert = @"
                        INSERT INTO solicitudes_becas
                            (UsuarioId, BecaId, BecaTitulo, BecaImporte,
                             FechaSolicitud, Estado, Motivacion, FechaResolucion,
                             CentroEducativo, Titulacion, AnioAcademico, NotaMediaODescripcion)
                        VALUES
                            (@uid, @bid, @titulo, @importe,
                             NOW(), 'Aprobada', @motiv, NOW(),
                             @centro, @titulac, @anio, @nota)";

                    int newId;
                    using (var cmd = new MySqlCommand(sqlInsert, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid",     usuarioId);
                        cmd.Parameters.AddWithValue("@bid",     beca.Id);
                        cmd.Parameters.AddWithValue("@titulo",  beca.Titulo);
                        cmd.Parameters.AddWithValue("@importe", beca.Importe);
                        cmd.Parameters.AddWithValue("@motiv",   motivacion          ?? "");
                        cmd.Parameters.AddWithValue("@centro",  centroEducativo     ?? "");
                        cmd.Parameters.AddWithValue("@titulac", titulacion          ?? "");
                        cmd.Parameters.AddWithValue("@anio",    anioAcademico       ?? "");
                        cmd.Parameters.AddWithValue("@nota",    notaMediaODescripcion ?? "");
                        cmd.ExecuteNonQuery();
                        newId = (int)cmd.LastInsertedId;
                    }

                    string numContrato = $"NXM-{DateTime.Now.Year}-{newId:D5}";

                    const string sqlUpdate =
                        "UPDATE solicitudes_becas SET NumeroContrato = @num WHERE Id = @id";
                    using (var cmd = new MySqlCommand(sqlUpdate, conn))
                    {
                        cmd.Parameters.AddWithValue("@num", numContrato);
                        cmd.Parameters.AddWithValue("@id",  newId);
                        cmd.ExecuteNonQuery();
                    }

                    return (true, numContrato, null);
                }
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public List<SolicitudBeca> ObtenerPorUsuario(int usuarioId)
        {
            var lista = new List<SolicitudBeca>();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    const string sql = @"
                        SELECT Id, UsuarioId, BecaId, BecaTitulo, BecaImporte,
                               FechaSolicitud, Estado, Motivacion,
                               NumeroContrato, FechaResolucion,
                               CentroEducativo, Titulacion, AnioAcademico, NotaMediaODescripcion
                        FROM solicitudes_becas
                        WHERE UsuarioId = @uid
                        ORDER BY FechaSolicitud DESC";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", usuarioId);
                        using (var r = cmd.ExecuteReader())
                            while (r.Read()) lista.Add(Mapear(r));
                    }
                }
            }
            catch { }
            return lista;
        }

        public bool YaSolicito(int usuarioId, int becaId)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    const string sql =
                        "SELECT COUNT(*) FROM solicitudes_becas WHERE UsuarioId=@uid AND BecaId=@bid";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", usuarioId);
                        cmd.Parameters.AddWithValue("@bid", becaId);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch { return false; }
        }

        private static SolicitudBeca Mapear(MySqlDataReader r)
        {
            return new SolicitudBeca
            {
                Id              = r.GetInt32("Id"),
                UsuarioId       = r.GetInt32("UsuarioId"),
                BecaId          = r.GetInt32("BecaId"),
                BecaTitulo      = r.GetString("BecaTitulo"),
                BecaImporte     = r.GetDecimal("BecaImporte"),
                FechaSolicitud  = r.GetDateTime("FechaSolicitud"),
                Estado          = r.GetString("Estado"),
                Motivacion             = r.IsDBNull(r.GetOrdinal("Motivacion"))             ? "" : r.GetString("Motivacion"),
                NumeroContrato         = r.IsDBNull(r.GetOrdinal("NumeroContrato"))         ? null : r.GetString("NumeroContrato"),
                FechaResolucion        = r.IsDBNull(r.GetOrdinal("FechaResolucion"))        ? (DateTime?)null : r.GetDateTime("FechaResolucion"),
                CentroEducativo        = r.IsDBNull(r.GetOrdinal("CentroEducativo"))        ? "" : r.GetString("CentroEducativo"),
                Titulacion             = r.IsDBNull(r.GetOrdinal("Titulacion"))             ? "" : r.GetString("Titulacion"),
                AnioAcademico          = r.IsDBNull(r.GetOrdinal("AnioAcademico"))          ? "" : r.GetString("AnioAcademico"),
                NotaMediaODescripcion  = r.IsDBNull(r.GetOrdinal("NotaMediaODescripcion"))  ? "" : r.GetString("NotaMediaODescripcion")
            };
        }
    }
}
