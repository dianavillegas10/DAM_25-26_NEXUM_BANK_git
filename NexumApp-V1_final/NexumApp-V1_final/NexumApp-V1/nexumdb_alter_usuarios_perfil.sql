-- Script para añadir columnas necesarias para el perfil mejorado (implementación Diana)
-- Ejecutar en phpMyAdmin sobre la base de datos nexumdb

ALTER TABLE `usuarios` ADD COLUMN `DNI` VARCHAR(20) DEFAULT NULL;
ALTER TABLE `usuarios` ADD COLUMN `Ciudad` VARCHAR(50) DEFAULT NULL;
ALTER TABLE `usuarios` ADD COLUMN `CodigoPostal` VARCHAR(10) DEFAULT NULL;
ALTER TABLE `usuarios` ADD COLUMN `FechaNacimiento` DATE DEFAULT NULL;
