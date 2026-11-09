/* =====================================================================
   Base de datos: PsicologiaDb  (SQL Server / LocalDB)
   Equivale a lo que crea Entity Framework en Program.cs.
   Es OPCIONAL: la aplicación crea la base de datos sola al iniciar.
   ===================================================================== */

IF DB_ID(N'PsicologiaDb') IS NULL
    CREATE DATABASE [PsicologiaDb];
GO

USE [PsicologiaDb];
GO

/* ---------- Tabla Roles ---------- */
IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        RoleId int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT PK_Roles PRIMARY KEY (RoleId)
    );
    CREATE UNIQUE INDEX IX_Roles_Name ON dbo.Roles ([Name]);

    SET IDENTITY_INSERT dbo.Roles ON;
    INSERT INTO dbo.Roles (RoleId, [Name]) VALUES
        (1, N'Administrador'),
        (2, N'Doctor'),
        (3, N'Paciente');
    SET IDENTITY_INSERT dbo.Roles OFF;
END
GO

/* ---------- Tabla Specialties ---------- */
IF OBJECT_ID(N'dbo.Specialties', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Specialties
    (
        SpecialtyId int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT PK_Specialties PRIMARY KEY (SpecialtyId)
    );
    CREATE UNIQUE INDEX IX_Specialties_Name ON dbo.Specialties ([Name]);

    SET IDENTITY_INSERT dbo.Specialties ON;
    INSERT INTO dbo.Specialties (SpecialtyId, [Name]) VALUES
        (1, N'Psicología Clínica'),
        (2, N'Neuropsicología');
    SET IDENTITY_INSERT dbo.Specialties OFF;
END
GO
