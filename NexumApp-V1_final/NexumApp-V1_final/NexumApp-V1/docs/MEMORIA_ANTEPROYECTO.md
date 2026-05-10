# MEMORIA DEL ANTEPROYECTO

---

## a) PORTADA

```
╔══════════════════════════════════════════════════════════════════╗
║                                                                  ║
║           CICLO FORMATIVO DE GRADO SUPERIOR                      ║
║     DESARROLLO DE APLICACIONES MULTIPLATAFORMA (DAM)             ║
║                                                                  ║
║                    Modalidad: Presencial                         ║
║                                                                  ║
║  ──────────────────────────────────────────────────────────────  ║
║                                                                  ║
║                       ANTEPROYECTO                               ║
║                                                                  ║
║                      NexumApp - V1                               ║
║               Aplicación Bancaria de Escritorio                  ║
║                                                                  ║
║  ──────────────────────────────────────────────────────────────  ║
║                                                                  ║
║                   EQUIPO DE DESARROLLO:                          ║
║                                                                  ║
║                   • Pablo [Apellidos]                            ║
║                   • Diana Villegas                               ║
║                   • [Nombre Compañero/a 3]                       ║
║                   • [Nombre Compañero/a 4]                       ║
║                                                                  ║
║  ──────────────────────────────────────────────────────────────  ║
║                                                                  ║
║                    Curso Académico 2025-2026                     ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
```

---

## b) TÍTULO DEL PROYECTO

**"NexumApp: Aplicación bancaria de escritorio con gestión de cuentas y transferencias"**

*(12 palabras)*

---

## c) DESCRIPCIÓN

### Descripción del Proyecto

NexumApp es una aplicación bancaria de escritorio desarrollada en C# con Windows Forms que simula las operaciones básicas de una entidad financiera. La aplicación permite a los usuarios gestionar sus cuentas bancarias, realizar movimientos de efectivo (ingresos y retiros) y ejecutar transferencias entre cuentas.

### Motivo de Realización

El proyecto surge de la necesidad de aplicar de forma práctica los conocimientos adquiridos durante el ciclo formativo, integrando múltiples tecnologías y conceptos en una aplicación funcional y realista. El sector bancario ofrece un contexto ideal para trabajar con:

- Bases de datos relacionales y gestión de información sensible
- Seguridad en el manejo de credenciales (cifrado de contraseñas)
- Interfaces de usuario intuitivas y profesionales
- Validación de datos y manejo de errores
- Arquitectura de software por capas

### Relevancia Técnica y Formativa

| Aspecto | Relevancia |
|---------|------------|
| **Técnica** | Implementación de patrones de diseño (Singleton, Repository), conexión a bases de datos MySQL, cifrado BCrypt, Entity Framework |
| **Formativa** | Trabajo en equipo, metodología de desarrollo, documentación técnica, control de versiones con Git |
| **Profesional** | Simulación de un entorno empresarial real, aplicación de buenas prácticas de desarrollo |

---

## d) OBJETIVOS

### Objetivo General

Desarrollar una aplicación bancaria de escritorio completa y funcional que permita la gestión integral de usuarios, cuentas bancarias, movimientos y transferencias, aplicando los conocimientos técnicos del ciclo formativo DAM.

### Objetivos Específicos

| # | Objetivo | Criterio de Evaluación |
|---|----------|------------------------|
| 1 | Implementar un sistema de autenticación seguro con cifrado de contraseñas | Login funcional con BCrypt, bloqueo tras intentos fallidos |
| 2 | Diseñar una interfaz de usuario moderna, intuitiva y responsive | Formularios con diseño premium, notificaciones tipo Toast |
| 3 | Crear un sistema de gestión de cuentas bancarias | CRUD completo de cuentas por usuario |
| 4 | Desarrollar el módulo de movimientos (ingresos/retiros) | Registro de movimientos con saldo actualizado |
| 5 | Implementar transferencias entre cuentas | Transferencias validadas con historial |
| 6 | Establecer conexión con base de datos MySQL | Persistencia de datos funcional |
| 7 | Aplicar control de versiones con Git/GitHub | Repositorio organizado con commits descriptivos |
| 8 | Documentar el proyecto técnicamente | README, comentarios, memoria |

### Indicadores de Éxito

- ✅ Usuario puede registrarse y acceder al sistema
- ✅ Usuario puede ver sus cuentas y saldos
- ✅ Usuario puede realizar ingresos y retiros
- ✅ Usuario puede hacer transferencias
- ✅ Administrador puede gestionar usuarios
- ✅ Datos persistentes en MySQL

---

## e) MÓDULOS PROFESIONALES IMPLICADOS

### Módulos de 1º Curso

| Módulo | Relación con el Proyecto |
|--------|--------------------------|
| **Programación** | Desarrollo de la lógica de negocio en C#, estructuras de control, POO, manejo de excepciones |
| **Bases de Datos** | Diseño del modelo relacional, consultas SQL, normalización de tablas |
| **Entornos de Desarrollo** | Uso de Visual Studio, depuración, control de versiones Git |
| **Lenguajes de Marcas** | Configuración XML (App.config), documentación en Markdown |
| **Sistemas Informáticos** | Instalación de XAMPP, configuración de servidor MySQL local |

### Módulos de 2º Curso

| Módulo | Relación con el Proyecto |
|--------|--------------------------|
| **Acceso a Datos** | Entity Framework, conexión MySQL, consultas LINQ, patrón Repository |
| **Desarrollo de Interfaces** | Windows Forms, diseño UX/UI, eventos, controles personalizados |
| **Programación Multimedia** | Recursos gráficos (logos, fondos), animaciones de interfaz |
| **Programación de Servicios y Procesos** | Gestión de sesiones, operaciones asíncronas |
| **Empresa e Iniciativa Emprendedora** | Documentación del proyecto, presentación, viabilidad |

### Justificación Técnica

```
┌─────────────────────────────────────────────────────────────────┐
│                        NexumApp                                 │
├─────────────────────────────────────────────────────────────────┤
│  Capa Presentación     │  Desarrollo de Interfaces (WinForms)  │
│  (Forms/)              │  Programación Multimedia (recursos)   │
├─────────────────────────────────────────────────────────────────┤
│  Capa Negocio          │  Programación (C#, POO)               │
│  (Services/)           │  Prog. Servicios (sesiones)           │
├─────────────────────────────────────────────────────────────────┤
│  Capa Datos            │  Acceso a Datos (EF, MySQL)           │
│  (Data/, Models/)      │  Bases de Datos (SQL, diseño)         │
├─────────────────────────────────────────────────────────────────┤
│  Infraestructura       │  Sistemas Informáticos (XAMPP)        │
│                        │  Entornos de Desarrollo (Git, VS)     │
└─────────────────────────────────────────────────────────────────┘
```

---

## f) METODOLOGÍA

### Metodología de Trabajo

El equipo utilizará una metodología **Agile simplificada** con sprints semanales:

1. **Reuniones semanales** para revisar avances y planificar tareas
2. **Control de versiones** con Git (ramas por funcionalidad)
3. **Revisión de código** entre compañeros antes de merge
4. **Documentación continua** del progreso

### Cronograma

| Fase | Semana | Tareas | Entregable |
|------|--------|--------|------------|
| **Fase 1: Planificación** | S1 | Definición de requisitos, diseño de BD, estructura del proyecto | Memoria anteproyecto, diagrama ER |
| **Fase 2: Base** | S2-S3 | Configuración entorno, BD MySQL, modelos, AuthService | Login y Registro funcional |
| **Fase 3: Core** | S4-S5 | Gestión de cuentas, movimientos (ingresos/retiros) | Módulo cuentas completo |
| **Fase 4: Avanzado** | S6-S7 | Transferencias, dashboard, panel admin | Funcionalidades completas |
| **Fase 5: Pulido** | S8 | Testing, corrección bugs, mejoras UI | Versión estable |
| **Fase 6: Entrega** | S9 | Documentación final, preparación defensa | Proyecto final |

### Diagrama de Gantt Simplificado

```
Semana:     S1    S2    S3    S4    S5    S6    S7    S8    S9
            ├─────┼─────┼─────┼─────┼─────┼─────┼─────┼─────┤
Fase 1      ████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
Fase 2      ░░░░████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
Fase 3      ░░░░░░░░░░░░░░░░████████████░░░░░░░░░░░░░░░░░░░░
Fase 4      ░░░░░░░░░░░░░░░░░░░░░░░░░░░░████████████░░░░░░░░
Fase 5      ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░████░░░░
Fase 6      ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░████
```

### Reparto de Funciones

| Miembro | Rol | Responsabilidades |
|---------|-----|-------------------|
| **Pablo** | Desarrollador Backend / BD | AuthService, conexión MySQL, modelos, seguridad |
| **Diana** | Desarrolladora Full Stack | Integración BD, servicios de cuentas/movimientos |
| **[Miembro 3]** | Desarrollador Frontend | Diseño de formularios, UX/UI, recursos gráficos |
| **[Miembro 4]** | QA / Documentación | Testing, documentación, preparación defensa |

### Recursos Necesarios

#### Recursos Técnicos (Software)

| Recurso | Versión | Uso |
|---------|---------|-----|
| Visual Studio | 2022 | IDE de desarrollo |
| .NET Framework | 4.7.2 | Runtime de la aplicación |
| XAMPP | 8.x | Servidor MySQL local |
| MySQL | 8.x | Base de datos |
| Git | 2.x | Control de versiones |
| GitHub | - | Repositorio remoto |

#### Recursos Técnicos (Hardware)

- Ordenadores con Windows 10/11
- Mínimo 8GB RAM
- 10GB espacio en disco

#### Recursos Humanos

- 4 estudiantes del ciclo DAM
- Tutor del proyecto (supervisión y orientación)

---

## 2. PRUEBA DE CONCEPTO (PoC)

### Descripción de la PoC

La Prueba de Concepto demuestra la viabilidad técnica del proyecto mediante la implementación funcional del módulo de autenticación:

### Funcionalidades Implementadas

| Funcionalidad | Estado | Descripción |
|---------------|--------|-------------|
| **Login** | ✅ Completo | Autenticación contra MySQL con mensajes específicos |
| **Registro** | ✅ Completo | Creación de usuarios con validaciones en tiempo real |
| **Cifrado** | ✅ Completo | Contraseñas hasheadas con BCrypt |
| **Sesión** | ✅ Completo | Gestión de sesión con patrón Singleton |
| **UI Premium** | ✅ Completo | Diseño moderno negro/dorado |
| **Notificaciones** | ✅ Completo | Sistema Toast sin MessageBox |
| **Bloqueo** | ✅ Completo | Bloqueo temporal tras 3 intentos fallidos |
| **Base de datos** | ✅ Completo | MySQL con 4 tablas relacionadas |

### Tecnologías Validadas

```
✅ C# / .NET Framework 4.7.2
✅ Windows Forms (UI)
✅ MySQL (via XAMPP)
✅ MySql.Data (conector)
✅ Entity Framework 6
✅ BCrypt.Net-Next (seguridad)
✅ Git/GitHub (versionado)
```

### Capturas de la PoC

*(Adjuntar capturas de pantalla del Login y Registro funcionando)*

1. Pantalla de Login
2. Validación de errores inline
3. Notificación Toast
4. Formulario de Registro
5. Indicador de fortaleza de contraseña
6. Base de datos en phpMyAdmin

### Cómo Ejecutar la PoC

1. Iniciar XAMPP (Apache + MySQL)
2. Importar `nexumdb.sql` en phpMyAdmin
3. Abrir `NexumApp.sln` en Visual Studio
4. Ejecutar (F5)
5. Probar login con: `admin@nexum.com` / `123456`
6. Probar registro creando un nuevo usuario

### Conclusión de Viabilidad

La PoC demuestra que:

- ✅ La conexión a MySQL funciona correctamente
- ✅ El cifrado BCrypt se integra sin problemas
- ✅ La arquitectura por capas es viable
- ✅ El diseño de UI premium es implementable en WinForms
- ✅ El equipo tiene las competencias técnicas necesarias

**El proyecto es técnicamente viable y se puede continuar con el desarrollo de las funcionalidades restantes.**

---

## ANEXOS

### Anexo A: Diagrama Entidad-Relación

```
┌─────────────┐       ┌──────────────────┐       ┌─────────────┐
│  USUARIOS   │       │ CUENTASBANCARIAS │       │ MOVIMIENTOS │
├─────────────┤       ├──────────────────┤       ├─────────────┤
│ Id (PK)     │──────<│ Id (PK)          │──────<│ Id (PK)     │
│ Nombre      │   1:N │ UsuarioId (FK)   │   1:N │ CuentaId(FK)│
│ Apellidos   │       │ NumeroCuenta     │       │ Tipo        │
│ Email       │       │ TipoCuenta       │       │ Monto       │
│ PasswordHash│       │ Saldo            │       │ Fecha       │
│ EsAdmin     │       │ FechaApertura    │       │ Concepto    │
│ Activo      │       │ Activa           │       └─────────────┘
└─────────────┘       └──────────────────┘
                              │
                              │ 1:N
                              ▼
                      ┌───────────────┐
                      │ TRANSFERENCIAS│
                      ├───────────────┤
                      │ Id (PK)       │
                      │ CuentaOrigen  │
                      │ CuentaDestino │
                      │ Monto         │
                      │ Fecha         │
                      │ Concepto      │
                      └───────────────┘
```

### Anexo B: Estructura del Proyecto

Ver archivo `README.md` en el repositorio.

### Anexo C: Repositorio

**GitHub:** https://github.com/[usuario]/NexumApp-V1

---

*Documento generado para el Anteproyecto del Ciclo Formativo DAM*
*Fecha: Febrero 2026*
