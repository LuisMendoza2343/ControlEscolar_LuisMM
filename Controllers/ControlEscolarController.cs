using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace Desarrollador.NET_LuisBarron.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControlEscolarController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // Inyecto IConfiguration para poder leer la cadena de conexión a SQL Server y la lista de caracteres a eliminar desde el appsettings.json
        public ControlEscolarController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ==========================================
        // 6. LOGIN DE ALUMNOS
        // Validar que sean caracteres alfabéticos y comprobar con la tabla Alumnos
        // ==========================================
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO modelo)
        {
            // Validamos con Expresión Regular que solo contenga letras y espacios (alfabéticos)
            string patronLetras = @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$";

            if (string.IsNullOrEmpty(modelo.Nombre) || !Regex.IsMatch(modelo.Nombre, patronLetras) ||
                string.IsNullOrEmpty(modelo.ApellidoPaterno) || !Regex.IsMatch(modelo.ApellidoPaterno, patronLetras))
            {
                return BadRequest(new { mensaje = "El nombre y apellido paterno solo deben contener caracteres alfabéticos." });
            }

            // Consultamos en SQL Server si existe el alumno
            string query = "SELECT idAlumno, nombre, apellidoPaterno FROM Alumnos WHERE nombre = @nombre AND apellidoPaterno = @apellidoPaterno";

            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@nombre", modelo.Nombre.Trim());
                cmd.Parameters.AddWithValue("@apellidoPaterno", modelo.ApellidoPaterno.Trim());

                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Ok(new
                        {
                            exito = true,
                            idAlumno = reader["idAlumno"],
                            nombreCompleto = $"{reader["nombre"]} {reader["apellidoPaterno"]}",
                            mensaje = "Autenticación exitosa"
                        });
                    }
                }
            }

            return Unauthorized(new { exito = false, mensaje = "El alumno no se encuentra registrado en el sistema." });
        }

        // ==========================================
        // 3. CRUD DE ALUMNOS (Altas, Bajas, Consultas)
        // ==========================================

        // Consultar todos los alumnos
        [HttpGet("alumnos")]
        public IActionResult ObtenerAlumnos()
        {
            List<AlumnoDTO> lista = new List<AlumnoDTO>();
            string query = "SELECT idAlumno, nombre, apellidoPaterno, apellidoMaterno FROM Alumnos";

            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                SqlCommand cmd = new SqlCommand(query, conexion);
                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new AlumnoDTO
                        {
                            IdAlumno = Convert.ToInt32(reader["idAlumno"]),
                            Nombre = reader["nombre"].ToString(),
                            ApellidoPaterno = reader["apellidoPaterno"].ToString(),
                            ApellidoMaterno = reader["apellidoMaterno"].ToString()
                        });
                    }
                }
            }
            return Ok(lista);
        }

        // Alta de Alumno
        [HttpPost("alumnos")]
        public IActionResult GuardarAlumno([FromBody] AlumnoDTO alumno)
        {
            string query = "INSERT INTO Alumnos (nombre, apellidoPaterno, apellidoMaterno) VALUES (@nombre, @apellidoPaterno, @apellidoMaterno)";

            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@nombre", alumno.Nombre);
                cmd.Parameters.AddWithValue("@apellidoPaterno", alumno.ApellidoPaterno);
                cmd.Parameters.AddWithValue("@apellidoMaterno", alumno.ApellidoMaterno);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Alumno registrado correctamente" });
        }

        // Baja de Alumno
        [HttpDelete("alumnos/{id}")]
        public IActionResult EliminarAlumno(int id)
        {
            string query = "DELETE FROM Alumnos WHERE idAlumno = @id";

            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@id", id);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Alumno eliminado correctamente" });
        }

        // ==========================================
        // 3 y 4. CRUD Y REGISTRO DE MATERIAS CON LIMPIEZA DE CARACTERES
        // ==========================================

        // Consultar todas las materias
        [HttpGet("materias")]
        public IActionResult ObtenerMaterias()
        {
            List<MateriaDTO> lista = new List<MateriaDTO>();
            string query = "SELECT idMateria, nombre, costo FROM Materias";

            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                SqlCommand cmd = new SqlCommand(query, conexion);
                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new MateriaDTO
                        {
                            IdMateria = Convert.ToInt32(reader["idMateria"]),
                            Nombre = reader["nombre"].ToString(),
                            Costo = Convert.ToDecimal(reader["costo"])
                        });
                    }
                }
            }
            return Ok(lista);
        }

        // Alta de Materia (Limpiando caracteres especiales configurados en appsettings.json)
        [HttpPost("materias")]
        public IActionResult GuardarMateria([FromBody] MateriaDTO materia)
        {
            // 1. Obtenemos la lista de caracteres del archivo de configuración ($\delta\alpha<>/$)
            string caracteresProhibidos = _configuration["CaracteresAEliminar"] ?? "$\\delta\\alpha<>/";

            // 2. Limpiamos el nombre reemplazando cada carácter prohibido por una cadena vacía
            string nombreLimpio = materia.Nombre;
            foreach (char c in caracteresProhibidos)
            {
                nombreLimpio = nombreLimpio.Replace(c.ToString(), "");
            }

            // 3. Insertamos el nombre limpio a la base de datos
            string query = "INSERT INTO Materias (nombre, costo) VALUES (@nombre, @costo)";

            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@nombre", nombreLimpio);
                cmd.Parameters.AddWithValue("@costo", materia.Costo);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Materia registrada exitosamente", nombreLimpio = nombreLimpio });
        }

        // Baja de Materia
        [HttpDelete("materias/{id}")]
        public IActionResult EliminarMateria(int id)
        {
            string query = "DELETE FROM Materias WHERE idMateria = @id";

            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@id", id);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Materia eliminada correctamente" });
        }

        // ==========================================
        // 4. ASIGNACIÓN DE MATERIAS A ALUMNOS Y CÁLCULO DE SUMA DE COSTO DESDE SQL
        // ==========================================

        // Registrar materia a un alumno
        [HttpPost("asignar-materia")]
        public IActionResult AsignarMateria([FromBody] AsignacionDTO asignacion)
        {
            string query = "INSERT INTO AlumnoMaterias (idAlumno, idMateria) VALUES (@idAlumno, @idMateria)";

            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@idAlumno", asignacion.IdAlumno);
                cmd.Parameters.AddWithValue("@idMateria", asignacion.IdMateria);

                conexion.Open();
                cmd.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Materia asignada al alumno correctamente" });
        }

        // Obtener la suma del costo de materias de un alumno llamando al Procedimiento Almacenado
        [HttpGet("suma-costo/{idAlumno}")]
        public IActionResult ObtenerSumaCosto(int idAlumno)
        {
            using (SqlConnection conexion = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerSumaCostoMaterias", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idAlumno", idAlumno);

                conexion.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Ok(new
                        {
                            idAlumno = reader["idAlumno"],
                            nombreAlumno = reader["NombreAlumno"].ToString(),
                            sumaTotalCosto = Convert.ToDecimal(reader["SuraTotalCosto"])
                        });
                    }
                }
            }
            return NotFound(new { mensaje = "No se encontraron registros para el alumno especificado." });
        }
    }

    // ==========================================
    // MODELOS DTO PARA LA TRANSFERENCIA DE DATOS
    // ==========================================
    public class LoginDTO
    {
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
    }

    public class AlumnoDTO
    {
        public int IdAlumno { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
    }

    public class MateriaDTO
    {
        public int IdMateria { get; set; }
        public string Nombre { get; set; }
        public decimal Costo { get; set; }
    }

    public class AsignacionDTO
    {
        public int IdAlumno { get; set; }
        public int IdMateria { get; set; }
    }
}