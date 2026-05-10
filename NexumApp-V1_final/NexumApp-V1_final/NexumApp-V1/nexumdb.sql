-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 26-02-2026 a las 21:38:09
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `nexumdb`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `cuentasbancarias`
--

CREATE TABLE `cuentas_bancarias` (
  `Id` int(11) NOT NULL,
  `UsuarioId` int(11) NOT NULL,
  `NumeroCuenta` varchar(20) NOT NULL,
  `TipoCuenta` varchar(20) NOT NULL,
  `Saldo` decimal(18,2) DEFAULT 0.00,
  `FechaApertura` datetime DEFAULT current_timestamp(),
  `Activa` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `cuentasbancarias`
--

INSERT INTO `cuentas_bancarias` (`Id`, `UsuarioId`, `NumeroCuenta`, `TipoCuenta`, `Saldo`, `FechaApertura`, `Activa`) VALUES
(1, 1, 'ES2100000000001', 'Corriente', 1000.00, '2026-02-25 21:57:11', 1),
(2, 2, 'ES2100000000002', 'Ahorro', 500.00, '2026-02-25 21:57:11', 1),
(3, 1, 'ES0000000001', 'Corriente', 1000.00, '2026-02-26 21:03:49', 1),
(4, 2, 'ES0000000002', 'Ahorro', 500.00, '2026-02-26 21:03:49', 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `movimientos`
--

CREATE TABLE `movimientos` (
  `Id` int(11) NOT NULL,
  `CuentaId` int(11) NOT NULL,
  `TipoMovimiento` varchar(20) NOT NULL,
  `Monto` decimal(18,2) NOT NULL,
  `Fecha` datetime DEFAULT current_timestamp(),
  `Concepto` varchar(200) DEFAULT NULL,
  `SaldoAnterior` decimal(18,2) NOT NULL,
  `SaldoPosterior` decimal(18,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `transferencias`
--

CREATE TABLE `transferencias` (
  `Id` int(11) NOT NULL,
  `CuentaOrigenId` int(11) NOT NULL,
  `CuentaDestino` varchar(20) NOT NULL,
  `NombreBeneficiario` varchar(100) DEFAULT NULL,
  `Monto` decimal(18,2) NOT NULL,
  `Fecha` datetime DEFAULT current_timestamp(),
  `Concepto` varchar(200) DEFAULT NULL,
  `Estado` varchar(20) DEFAULT 'Completada'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuarios`
--

CREATE TABLE `usuarios` (
  `Id` int(11) NOT NULL,
  `Nombre` varchar(50) NOT NULL,
  `Apellidos` varchar(100) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `Telefono` varchar(20) DEFAULT NULL,
  `DNI` varchar(20) DEFAULT NULL,
  `Direccion` varchar(200) DEFAULT NULL,
  `Ciudad` varchar(50) DEFAULT NULL,
  `CodigoPostal` varchar(10) DEFAULT NULL,
  `FechaNacimiento` date DEFAULT NULL,
  `FechaRegistro` datetime DEFAULT current_timestamp(),
  `UltimoAcceso` datetime DEFAULT NULL,
  `EsAdmin` tinyint(1) DEFAULT 0,
  `Activo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `usuarios`
--

INSERT INTO `usuarios` (`Id`, `Nombre`, `Apellidos`, `Email`, `PasswordHash`, `Telefono`, `Direccion`, `FechaRegistro`, `UltimoAcceso`, `EsAdmin`, `Activo`) VALUES
(1, 'Admin', 'Sistema', 'admin@nexum.com', '$2a$11$gA5EeUNvzHhWOQtMkWuemunt9TvO4eJcC2//8UCZbtVCRyN4R7bti', NULL, NULL, '2026-02-25 21:57:11', NULL, 1, 1),
(2, 'Juan', 'Pérez', 'juan@email.com', '$2a$11$gA5EeUNvzHhWOQtMkWuemunt9TvO4eJcC2//8UCZbtVCRyN4R7bti', NULL, NULL, '2026-02-25 21:57:11', NULL, 0, 1),
(4, 'Usuario', 'Prueba', 'test@test.com', '$2a$11$gA5EeUNvzHhWOQtMkWuemunt9TvO4eJcC2//8UCZbtVCRyN4R7bti', NULL, NULL, '2026-02-26 21:03:49', NULL, 0, 1);

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `cuentas_bancarias`
--
ALTER TABLE `cuentas_bancarias`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `NumeroCuenta` (`NumeroCuenta`),
  ADD KEY `UsuarioId` (`UsuarioId`);

--
-- Indices de la tabla `movimientos`
--
ALTER TABLE `movimientos`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `CuentaId` (`CuentaId`);

--
-- Indices de la tabla `transferencias`
--
ALTER TABLE `transferencias`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `CuentaOrigenId` (`CuentaOrigenId`);

--
-- Indices de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Email` (`Email`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `cuentas_bancarias`
--
ALTER TABLE `cuentas_bancarias`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT de la tabla `movimientos`
--
ALTER TABLE `movimientos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `transferencias`
--
ALTER TABLE `transferencias`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `cuentas_bancarias`
--
ALTER TABLE `cuentas_bancarias`
  ADD CONSTRAINT `cuentas_bancarias_ibfk_1` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `movimientos`
--
ALTER TABLE `movimientos`
  ADD CONSTRAINT `movimientos_ibfk_1` FOREIGN KEY (`CuentaId`) REFERENCES `cuentas_bancarias` (`Id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `transferencias`
--
ALTER TABLE `transferencias`
  ADD CONSTRAINT `transferencias_ibfk_1` FOREIGN KEY (`CuentaOrigenId`) REFERENCES `cuentas_bancarias` (`Id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
