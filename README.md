# Examen Práctico de Reclutamiento - Desarrollador .NET

**Candidato:** Luis Barrón  
**Tecnologías:** ASP.NET Core Web API, SQL Server, HTML5, Bootstrap 5, jQuery & AJAX.

---

## 📌 Descripción del Proyecto

Este proyecto es una aplicación web de **Control Escolar** desarrollada para la prueba técnica de reclutamiento de DigiPro. Permite gestionar alumnos, materias, asignaciones y realizar el cálculo automatizado del costo total de materias registradas mediante un procedimiento almacenado en SQL Server.

---

## 🛠️ Características Principales

1. **Validación de Inicio de Sesión (Regex):** 
   - Validación estricta para aceptar únicamente caracteres alfanuméricos (letras y números), rechazando símbolos o caracteres especiales.
2. **Sanitización de Caracteres Especiales (appsettings.json):**
   - Configuración centralizada para filtrar y remover caracteres no permitidos (`&<>/`) en el nombre de las materias antes de realizar el registro en la base de datos.
3. **Gestión CRUD:**
   - Alta, consulta y eliminación de Alumnos y Materias.
   - Eliminación en cascada (`ON DELETE CASCADE`) implementada en la base de datos para mantener la integridad relacional sin bloqueos.
4. **Asignación y Cálculo de Costos:**
   - Asignación dinámica de materias a alumnos.
   - Consulta del costo total mediante la ejecución del procedimiento almacenado `sp_ObtenerSumaCostoMaterias`.
5. **Interfaz Dinámica (Single Page Application):**
   - Construida con HTML5, Bootstrap 5 y consumo de la API mediante peticiones AJAX con jQuery.

---

## 🗄️ Requisitos e Instalación de la Base de Datos

1. Abre tu gestor de base de datos **SQL Server Management Studio (SSMS)**.
2. Localiza el archivo `ControlEscolar_Script.sql` que se encuentra en la raíz de este repositorio.
3. Ejecuta el script completo para crear la base de datos `ControlEscolar`, sus tablas (`Alumnos`, `Materias`, `AlumnoMaterias`) y el procedimiento almacenado `sp_ObtenerSumaCostoMaterias`.

---

## 🚀 Configuración y Ejecución de la API

1. Abre la solución `.sln` en **Visual Studio**.
2. Verifica la cadena de conexión en el archivo `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "CadenaSQL": "Server=TU_SERVIDOR;Database=ControlEscolar;Trusted_Connection=True;TrustServerCertificate=True;"
   }
