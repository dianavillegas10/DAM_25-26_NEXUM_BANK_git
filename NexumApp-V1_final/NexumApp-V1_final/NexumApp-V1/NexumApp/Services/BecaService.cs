using NexumApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NexumApp.Services
{
    /// <summary>
    /// Servicio de becas. Datos de muestra listos para conectar a BD.
    /// </summary>
    public class BecaService
    {
        private static readonly List<Beca> _catalogo = new List<Beca>
        {
            new Beca
            {
                Id = 1,
                Titulo = "Beca Excelencia Universitaria",
                Descripcion = "Para estudiantes universitarios con expediente brillante. Cubre matrícula y ayuda al estudio en cualquier universidad española.",
                Importe = 8000,
                Categoria = CategoriaBeca.Universitaria,
                Estado = EstadoSolicitud.Abierta,
                FechaCierre = new DateTime(2026, 7, 15),
                Requisitos = "Expediente académico ≥ 8,0 y matrícula en grado oficial",
                EntidadConvocante = "Nexum Bank",
                Destacada = true,
                PlazasDisponibles = 50,
                DuracionTexto = "Curso completo"
            },
            new Beca
            {
                Id = 2,
                Titulo = "Beca Emprende Digital",
                Descripcion = "Impulsa tu idea tecnológica. Financiamos proyectos digitales innovadores de jóvenes emprendedores de 18 a 30 años.",
                Importe = 5000,
                Categoria = CategoriaBeca.Digital,
                Estado = EstadoSolicitud.Abierta,
                FechaCierre = new DateTime(2026, 6, 30),
                Requisitos = "18-30 años con proyecto digital o startup",
                EntidadConvocante = "Nexum Innovation",
                Destacada = false,
                PlazasDisponibles = 20,
                DuracionTexto = "6 meses"
            },
            new Beca
            {
                Id = 3,
                Titulo = "Beca Máster Internacional",
                Descripcion = "Financia tu máster en el extranjero. Cubrimos gastos de matrícula, alojamiento y manutención en universidades de primer nivel.",
                Importe = 12000,
                Categoria = CategoriaBeca.Posgrado,
                Estado = EstadoSolicitud.Abierta,
                FechaCierre = new DateTime(2026, 7, 31),
                Requisitos = "Título universitario y admisión en máster internacional",
                EntidadConvocante = "Fundación Nexum",
                Destacada = false,
                PlazasDisponibles = 15,
                DuracionTexto = "1 año académico"
            },
            new Beca
            {
                Id = 4,
                Titulo = "Beca FP Tecnológico",
                Descripcion = "Para estudiantes de Formación Profesional en ramas tecnológicas. Apoyamos la formación dual y prácticas en empresa.",
                Importe = 3500,
                Categoria = CategoriaBeca.FP,
                Estado = EstadoSolicitud.Abierta,
                FechaCierre = new DateTime(2026, 6, 30),
                Requisitos = "Matriculado en FP de grado superior rama TIC o industria",
                EntidadConvocante = "Nexum Bank",
                Destacada = false,
                PlazasDisponibles = 80,
                DuracionTexto = "Curso completo"
            },
            new Beca
            {
                Id = 5,
                Titulo = "Beca Deporte y Estudios",
                Descripcion = "Compatibiliza el alto rendimiento deportivo con tu formación académica. Para deportistas federados con licencia activa.",
                Importe = 4000,
                Categoria = CategoriaBeca.Deportiva,
                Estado = EstadoSolicitud.Abierta,
                FechaCierre = new DateTime(2026, 8, 31),
                Requisitos = "Licencia federativa activa y matrícula en enseñanza oficial",
                EntidadConvocante = "Nexum & Deporte",
                Destacada = false,
                PlazasDisponibles = 30,
                DuracionTexto = "Temporada"
            },
            new Beca
            {
                Id = 6,
                Titulo = "Beca Arte y Creatividad",
                Descripcion = "Para artistas emergentes en artes plásticas, música, diseño y comunicación audiovisual. Incluye mentoría profesional.",
                Importe = 2500,
                Categoria = CategoriaBeca.Arte,
                Estado = EstadoSolicitud.Abierta,
                FechaCierre = new DateTime(2026, 6, 30),
                Requisitos = "Portfolio artístico y carta de motivación",
                EntidadConvocante = "Fundación Nexum Cultural",
                Destacada = false,
                PlazasDisponibles = 25,
                DuracionTexto = "9 meses"
            },
            new Beca
            {
                Id = 7,
                Titulo = "Beca Doctorado Investigación",
                Descripcion = "Financia tu tesis doctoral en áreas STEM, ciencias sociales o humanidades. Acceso a red de investigadores Nexum.",
                Importe = 15000,
                Categoria = CategoriaBeca.Posgrado,
                Estado = EstadoSolicitud.Abierta,
                FechaCierre = new DateTime(2026, 7, 15),
                Requisitos = "Admisión en programa de doctorado oficial",
                EntidadConvocante = "Fundación Nexum",
                Destacada = false,
                PlazasDisponibles = 5,
                DuracionTexto = "3 años"
            }
        };

        public List<Beca> ObtenerTodas()
        {
            return _catalogo;
        }

        public List<Beca> ObtenerPorCategoria(CategoriaBeca categoria)
        {
            return _catalogo.Where(b => b.Categoria == categoria).ToList();
        }

        public List<Beca> ObtenerAbiertas()
        {
            return _catalogo.Where(b => b.Estado == EstadoSolicitud.Abierta).ToList();
        }

        public Beca ObtenerDestacada()
        {
            return _catalogo.FirstOrDefault(b => b.Destacada && b.Estado == EstadoSolicitud.Abierta);
        }

        public int ContarAbiertas()
        {
            return _catalogo.Count(b => b.Estado == EstadoSolicitud.Abierta);
        }

        public decimal ImporteTotalDisponible()
        {
            return _catalogo.Where(b => b.Estado == EstadoSolicitud.Abierta).Sum(b => b.Importe);
        }
    }
}
