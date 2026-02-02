-- ----------------------------------------------------
-- Script de Base de Datos para ProyectoWebCoworking
-- Autor: David Suárez-Otero Redondo
-- ----------------------------------------------------

-- 1. Borramos la base de datos si existiese para empezar de cero.
DROP DATABASE IF EXISTS coworking_db;
CREATE DATABASE coworking_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE coworking_db;

-- 2. Creamos las tablas
CREATE TABLE  Usuarios ( 
Id INT AUTO_INCREMENT PRIMARY KEY,
Nombre VARCHAR (100) NOT NULL,
Apellidos VARCHAR (100) NOT NULL DEFAULT '',
Email VARCHAR (100) NOT NULL UNIQUE,
ContraseñaHash VARCHAR (255) NOT NULL,
Rol VARCHAR (20) NOT NULL DEFAULT 'Cliente',
Teléfono VARCHAR (20) NULL
);
CREATE TABLE Recursos (
Id INT AUTO_INCREMENT PRIMARY KEY,
Nombre VARCHAR (100) NOT NULL,
Capacidad INT NOT NULL DEFAULT 1,
Tipo VARCHAR (50) NOT NULL
);
CREATE TABLE Tarifas (
Id INT AUTO_INCREMENT PRIMARY KEY,
Nombre VARCHAR (100) NOT NULL,
Precio  DECIMAL (10, 2) NOT NULL,
TipoRecurso VARCHAR (50) NOT NULL
);
CREATE TABLE Reservas (
Id INT AUTO_INCREMENT PRIMARY KEY,
UsuarioId INT NOT NULL,
RecursoId INT NOT NULL,
TarifaId INT NOT NULL,
FechaHoraInicio DATETIME NOT NULL,
FechaHoraFin DATETIME NOT NULL,
Estado VARCHAR (30) NOT NULL DEFAULT 'Confirmada',

FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id),
FOREIGN KEY (RecursoId) REFERENCES Recursos (Id),
FOREIGN KEY (TarifaId) REFERENCES Tarifas (Id)
);

-- 3. Insertamos datos de prueba
INSERT INTO usuarios (Nombre, Apellidos, Email, ContraseñaHash, Rol, Teléfono) VALUES 
('Administrador', 'Principal', 'admin@coworking.es', '$2a$11$mfzf0ZqXY56bueWQ6Nog/ebnVOq9YUloyGjPCj7E6ufEllTgIwmzG', 'Administrador', '600000000'),
('Cliente', 'Prueba', 'cliente@test.es', '$2a$11$PWeZDJtcWSUx7Q2h5kEhpuGGhPgFY9thrwnGqFGCwVk9jJlEGmXwW', 'Cliente', '600111222');
-- NOTA: La contraseña del administrador es "admin" y la del cliente es "cliente"

INSERT INTO recursos (Nombre, Tipo, Capacidad) VALUES 
('Sala de Juntas', 'Sala', 10),
('Sala Creativa', 'Sala', 6),
('Puesto Fijo 1', 'Puesto', 1),
('Puesto Fijo 2', 'Puesto', 1),
('Despacho Privado', 'Despacho', 3);

INSERT INTO tarifas (Nombre, Precio, TipoRecurso) VALUES 
('Tarifa Sala Estándar', 150.00, 'Sala'),
('Tarifa Puesto Diario', 15.00, 'Puesto'),
('Tarifa Despacho', 50.00, 'Despacho');