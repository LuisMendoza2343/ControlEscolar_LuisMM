-- ========================================================
-- SCRIPT COMPLETO DE BASE DE DATOS - EXAMEN DIGIPRO - LUIS MOLIONA MENDOZA
-- Base de datos: ControlEscolar
-- ========================================================

CREATE DATABASE ControlEscolar;
GO

USE ControlEscolar;
GO

-- 1. Tabla de Alumnos (Requerido para el Login y CRUD)
CREATE TABLE Alumnos (
    idAlumno INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    apellidoPaterno VARCHAR(100) NOT NULL,
    apellidoMaterno VARCHAR(100) NULL
);
GO

-- 2. Tabla de Materias 
CREATE TABLE Materias (
    idMateria INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(150) NOT NULL,
    costo DECIMAL(18,2) NOT NULL
);
GO

-- 3. Tabla intermedia para la Asignación 
CREATE TABLE AlumnoMaterias (
    idAsignacion INT IDENTITY(1,1) PRIMARY KEY,
    idAlumno INT NOT NULL,
    idMateria INT NOT NULL,
    CONSTRAINT FK_AlumnoMat_IdAlu FOREIGN KEY (idAlumno) REFERENCES Alumnos(idAlumno) ON DELETE CASCADE,
    CONSTRAINT FK_AlumnoMat_IdMat FOREIGN KEY (idMateria) REFERENCES Materias(idMateria) ON DELETE CASCADE
);
GO

-- 4. Procedimiento Almacenado para calcular la suma de costos 
CREATE PROCEDURE sp_ObtenerSumaCostoMaterias
    @idAlumno INT
AS
BEGIN
    SELECT 
        A.nombre + ' ' + A.apellidoPaterno AS Alumno,
        ISNULL(SUM(M.costo), 0) AS SumaTotalCosto
    FROM Alumnos A
    LEFT JOIN AlumnoMaterias AM ON A.idAlumno = AM.idAlumno
    LEFT JOIN Materias M ON AM.idMateria = M.idMateria
    WHERE A.idAlumno = @idAlumno
    GROUP BY A.nombre, A.apellidoPaterno;
END;
GO

-- 5. Datos iniciales de prueba, INSERCION DE DTOS
INSERT INTO Alumnos (nombre, apellidoPaterno, apellidoMaterno) 
VALUES ('Luis', 'Mendoza', 'Perez');
GO

INSERT INTO Materias (nombre, costo) 
VALUES ('Matematicas', 150.50), ('Espanol', 100.00);
GO