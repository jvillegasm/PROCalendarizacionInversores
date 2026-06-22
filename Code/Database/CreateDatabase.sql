-- ============================================================
-- 01_CreateDatabase.sql
-- Conectar a: master
-- Compatible: SQL Server 2019/2022 y Azure SQL Database
-- ============================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'PROInversores')
BEGIN
    CREATE DATABASE PROInversores COLLATE Latin1_General_CI_AI;
    PRINT 'Base de datos PROInversores creada.';
END
ELSE
    PRINT 'PROInversores ya existe.';
GO

PRINT '============================================================';
PRINT 'Siguiente paso: conectar a PROInversores y ejecutar';
PRINT '               02_Schema_And_Data.sql';
PRINT '============================================================';
GO
