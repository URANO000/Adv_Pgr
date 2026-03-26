-- =============================================
-- PATITAS SOCIAL  Script completo
-- Tablas + SPs + Datos de prueba
-- =============================================

DROP DATABASE IF EXISTS patitassocial;
GO

CREATE DATABASE patitassocial;
GO

USE patitassocial;
GO

-- =============================================
-- 1. TABLAS BASE
-- =============================================

CREATE TABLE Rol (
    RolId INT IDENTITY PRIMARY KEY,
    NombreRol NVARCHAR(100) NOT NULL
);

CREATE TABLE AnimalCategoria (
    CategoriaId INT IDENTITY PRIMARY KEY,
    NombreTipo NVARCHAR(100)
);

-- =============================================
-- 2. TABLAS CON DEPENDENCIAS
-- =============================================

CREATE TABLE Usuario (
    UsuarioId NVARCHAR(450) PRIMARY KEY DEFAULT NEWID(),
    CorreoElectronico NVARCHAR(255) NOT NULL UNIQUE,
    ContrasenaHash NVARCHAR(500) NOT NULL,
    PrimerNombre NVARCHAR(100) NOT NULL,
    SegundoNombre NVARCHAR(100) NULL,
    PrimerApellido NVARCHAR(100) NOT NULL,
    SegundoApellido NVARCHAR(100) NULL,
    Cedula NVARCHAR(200) NULL,
    Telefono NVARCHAR(30) NULL,
    Provincia NVARCHAR(100) NULL,
    ImagenPerfil VARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    IsActive BIT NOT NULL DEFAULT 1,
    RolId INT NOT NULL,

    CONSTRAINT [FK_Usuario_Rol] FOREIGN KEY (RolId) REFERENCES Rol(RolId)
);

CREATE TABLE AnimalTipo (
    TipoId INT IDENTITY PRIMARY KEY,
    NombreTipo NVARCHAR(100),
    CategoriaId INT,

    CONSTRAINT [FK_AnimalTipo_Categoria] FOREIGN KEY (CategoriaId) REFERENCES AnimalCategoria(CategoriaId)
);

CREATE TABLE Animal (
    AnimalId INT IDENTITY PRIMARY KEY,
    UsuarioId NVARCHAR(450) NOT NULL,
    TipoId INT NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Peso DECIMAL(5,2) NULL,
    Edad NVARCHAR(100) NULL,
    Sexo CHAR(1) NULL CHECK (Sexo IN('M','H')),
    Enfermedades NVARCHAR(MAX) NULL,
    HistorialMedico NVARCHAR(MAX) NULL,
    PreferenciasAlimenticias NVARCHAR(MAX) NULL,
    Notas NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CreatedBy NVARCHAR(450) NULL,

    CONSTRAINT [FK_Animal_Usuario] FOREIGN KEY (UsuarioId) REFERENCES Usuario(UsuarioId),
    CONSTRAINT [FK_Animal_CreatedBy] FOREIGN KEY (CreatedBy) REFERENCES Usuario(UsuarioId),
    CONSTRAINT [FK_Animal_Tipo] FOREIGN KEY (TipoId) REFERENCES AnimalTipo(TipoId)
);

CREATE TABLE AnimalMedia (
    MediaId INT IDENTITY PRIMARY KEY NOT NULL,
    AnimalId INT NOT NULL,
    ArchivoUrl VARCHAR(MAX) NULL,
    FileSize BIGINT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CreatedBy NVARCHAR(450) NULL,

    CONSTRAINT [FK_AnimalMedia_Animal] FOREIGN KEY (AnimalId) REFERENCES Animal(AnimalId),
    CONSTRAINT [FK_AnimalMedia_Usuario] FOREIGN KEY (CreatedBy) REFERENCES Usuario(UsuarioId)
);

CREATE TABLE Publicacion (
    PublicacionId INT IDENTITY PRIMARY KEY NOT NULL,
    Titulo NVARCHAR(200) NULL,
    Descripcion NVARCHAR(300) NULL,
    AnimalId INT NOT NULL,
    IsActive BIT NULL,
    PublishedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    PublishedBy NVARCHAR(450) NULL,
    UpdatedAt DATETIME2 NULL DEFAULT SYSDATETIME(),
    UpdatedBy NVARCHAR(450) NULL,
    ClosedAt DATETIME2 NULL,

    CONSTRAINT [FK_Publicacion_Animal] FOREIGN KEY (AnimalId) REFERENCES Animal(AnimalId),
    CONSTRAINT [FK_Publicacion_PublishedBy] FOREIGN KEY (PublishedBy) REFERENCES Usuario(UsuarioId),
    CONSTRAINT [FK_Publicacion_UpdatedBy] FOREIGN KEY (UpdatedBy) REFERENCES Usuario(UsuarioId)
);

CREATE TABLE Solicitud (
    Id INT IDENTITY PRIMARY KEY,
    PublicacionId INT NOT NULL,
    UsuarioInteresadoId NVARCHAR(450) NOT NULL,
    Mensaje NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT [FK_Solicitud_Publicacion] FOREIGN KEY (PublicacionId) REFERENCES Publicacion(PublicacionId),
    CONSTRAINT [FK_Solicitud_Usuario] FOREIGN KEY (UsuarioInteresadoId) REFERENCES Usuario(UsuarioId)
);

CREATE TABLE Fundraiser (
    FundraiserId INT IDENTITY PRIMARY KEY,
    AnimalId INT NOT NULL,
    Titulo NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(MAX) NOT NULL,
    MetaTotal DECIMAL(10,2) NOT NULL,
    TotalActual DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT [FK_Fundraiser_Animal] FOREIGN KEY (AnimalId) REFERENCES Animal(AnimalId)
);

CREATE TABLE Donacion (
    DonacionId INT IDENTITY PRIMARY KEY,
    FundraiserId INT NOT NULL,
    UsuarioId NVARCHAR(450) NULL,
    Total DECIMAL(10,2) NOT NULL,
    DonatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT [FK_Donacion_Fundraiser] FOREIGN KEY (FundraiserId) REFERENCES Fundraiser(FundraiserId),
    CONSTRAINT [FK_Donacion_Usuario] FOREIGN KEY (UsuarioId) REFERENCES Usuario(UsuarioId)
);
GO

-- =============================================
-- 3. DATOS INICIALES
-- =============================================

INSERT INTO Rol (NombreRol) VALUES ('Administrador'), ('Usuario Normal');

INSERT INTO AnimalCategoria (NombreTipo) VALUES ('Caninos'), ('Felinos'), ('Roedores'), ('Aves');

INSERT INTO AnimalTipo (NombreTipo, CategoriaId) VALUES
('Perro',     1),
('Gato',      2),
('Conejo',    3),
('Hamster',   3),
('Periquito', 4);
GO

-- =============================================
-- 4. SPs â AutenticaciÃ³n (Adriana)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_RegistrarCuenta]
	@CorreoElectronico NVARCHAR(255),
	@Contrasenna NVARCHAR(500),
	@PrimerNombre NVARCHAR(100),
	@SegundoNombre NVARCHAR(100) NULL,
	@PrimerApellido NVARCHAR(100),
	@SegundoApellido NVARCHAR(100) NULL,
	@Cedula NVARCHAR(200),
	@Telefono NVARCHAR(30) NULL,
	@Provincia NVARCHAR(100) NULL,
	@CreatedAt DATETIME2
AS
BEGIN
	IF NOT EXISTS (
		SELECT 1 FROM Usuario
		WHERE Cedula = @Cedula
		OR CorreoElectronico = @CorreoElectronico)
	BEGIN

		INSERT INTO [dbo].[Usuario] (CorreoElectronico,ContrasenaHash, PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido, Cedula, Telefono, Provincia, CreatedAt, IsActive, RolId)
		VALUES (@CorreoElectronico, @Contrasenna, @PrimerNombre, @SegundoNombre, @PrimerApellido, @SegundoApellido, @Cedula, @Telefono, @Provincia, @CreatedAt, 1, 2)

	END
END
GO

CREATE OR ALTER PROCEDURE  [dbo].[sp_IniciarSesion]
	@CorreoElectronico NVARCHAR(255),
	@Contrasenna NVARCHAR(500)
AS
BEGIN

	SELECT u.UsuarioId, u.CorreoElectronico, u.PrimerNombre, u.SegundoNombre,
	u.PrimerApellido, u.SegundoApellido, u.Cedula, u.Telefono, u.Provincia, u.CreatedAt, u.IsActive, u.ImagenPerfil,
	r.NombreRol
	FROM Usuario u
	INNER JOIN Rol r ON u.RolId = r.RolId
	WHERE CorreoElectronico = @CorreoElectronico
		AND ContrasenaHash = @Contrasenna
		AND IsActive = 1
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ActualizarContrasenna]
	@UsuarioId NVARCHAR(450),
	@Contrasenna NVARCHAR(500)
AS
BEGIN
	UPDATE [dbo].[Usuario]
	SET ContrasenaHash = @Contrasenna
	WHERE UsuarioId = @UsuarioId
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ValidarCorreo]
	@CorreoElectronico NVARCHAR(255)
AS
BEGIN
	SELECT UsuarioId, CorreoElectronico, PrimerNombre, SegundoNombre,
	PrimerApellido, SegundoApellido, Cedula, Telefono, Provincia, CreatedAt, IsActive,
	RolId
	FROM Usuario
	WHERE CorreoElectronico = @CorreoElectronico
	AND IsActive = 1
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ObtenerUsuario]
	@UsuarioId NVARCHAR(450)
AS
BEGIN
	SELECT 
		u.UsuarioId,
		u.CorreoElectronico,
		u.PrimerNombre,
		u.SegundoNombre,
		u.PrimerApellido,
		u.SegundoApellido,
		u.Cedula,
		u.Telefono,
		u.Provincia,
		u.ImagenPerfil,
		u.IsActive,
		u.RolId
	FROM Usuario u
	WHERE u.UsuarioId = @UsuarioId
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ObtenerUsuarios]
AS
BEGIN
	SELECT UsuarioId, CorreoElectronico, PrimerNombre, SegundoNombre,
	PrimerApellido, SegundoApellido, Cedula, Telefono, Provincia, CreatedAt,
	RolId, ImagenPerfil, IsActive
	FROM Usuario
	ORDER BY CreatedAt 
END
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_EditarUsuario]
	@UsuarioId NVARCHAR(450),
	@CorreoElectronico NVARCHAR(255),
	@PrimerNombre NVARCHAR(100),
	@SegundoNombre NVARCHAR(100) NULL,
	@PrimerApellido NVARCHAR(100),
	@SegundoApellido NVARCHAR(100) NULL,
	@Cedula NVARCHAR(200),
	@Telefono NVARCHAR(30) NULL,
	@Provincia NVARCHAR(100) NULL,

	@ImagenPerfil VARCHAR(MAX) NULL
AS
BEGIN

	UPDATE dbo.Usuario
	SET CorreoElectronico = @CorreoElectronico,
		PrimerNombre = @PrimerNombre,
		SegundoNombre = @SegundoNombre,
		PrimerApellido = @PrimerApellido,
		SegundoApellido = @SegundoApellido,
		Cedula = @Cedula,
		Telefono = @Telefono,
		Provincia = @Provincia,
		ImagenPerfil = @ImagenPerfil

	WHERE UsuarioId = @UsuarioId;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_DesactivarUsuario]
	@UsuarioId NVARCHAR(450)
AS
BEGIN
	UPDATE [dbo].[Usuario]
	SET IsActive = 0
	WHERE UsuarioId = @UsuarioId
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ActivarUsuario]
	@UsuarioId NVARCHAR(450)
AS
BEGIN
	UPDATE [dbo].[Usuario]
	SET IsActive = 1
	WHERE UsuarioId = @UsuarioId
END
GO

-- =============================================
-- 5. SPs â Animales
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_ListarAnimales]
AS
BEGIN
    SELECT a.AnimalId,
           a.Nombre + ' (' + t.NombreTipo + ')' AS Nombre
    FROM Animal a
    JOIN AnimalTipo t ON t.TipoId = a.TipoId
    ORDER BY a.Nombre;
END
GO

-- =============================================
-- 6. SPs â Fundraiser (Evelyn: RF-015,016,018,019)
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_RegistrarFundraiser]
    @AnimalId INT,
    @Titulo NVARCHAR(200),
    @Descripcion NVARCHAR(MAX),
    @MetaTotal DECIMAL(10,2)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Animal WHERE AnimalId = @AnimalId)
    BEGIN
        INSERT INTO Fundraiser (AnimalId, Titulo, Descripcion, MetaTotal, TotalActual, IsActive)
        VALUES (@AnimalId, @Titulo, @Descripcion, @MetaTotal, 0, 1);
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_EditarFundraiser]
    @FundraiserId INT,
    @AnimalId INT,
    @Titulo NVARCHAR(200),
    @Descripcion NVARCHAR(MAX)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Fundraiser WHERE FundraiserId = @FundraiserId AND IsActive = 1)
    BEGIN
        UPDATE Fundraiser
        SET AnimalId    = @AnimalId,
            Titulo      = @Titulo,
            Descripcion = @Descripcion
        WHERE FundraiserId = @FundraiserId;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_InactivarFundraiser]
    @FundraiserId INT
AS
BEGIN
    UPDATE Fundraiser SET IsActive = 0
    WHERE FundraiserId = @FundraiserId AND IsActive = 1;
END
GO

CREATE PROCEDURE sp_ValidarCampanaActivaPorAnimal
    @AnimalId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM Fundraiser 
        WHERE AnimalId = @AnimalId AND IsActive = 1
    )
        SELECT CAST(1 AS BIT) AS TieneCampanaActiva;
    ELSE
        SELECT CAST(0 AS BIT) AS TieneCampanaActiva;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarAnimalesPorUsuario
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.AnimalId,
           a.Nombre + ' (' + t.NombreTipo + ')' AS Nombre
    FROM Animal a
    JOIN AnimalTipo t ON t.TipoId = a.TipoId
    WHERE a.UsuarioId = @UsuarioId
    ORDER BY a.Nombre;
END
GO

-- =============================================
-- 7. SPs â Fundraiser (Isaac: RF-017,020,021,022,030)
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[sp_ObtenerFundraiser]
    @FundraiserId INT
AS
BEGIN
    SELECT f.FundraiserId, f.AnimalId, a.Nombre AS NombreAnimal,
           f.Titulo, f.Descripcion, f.MetaTotal, f.TotalActual,
           f.IsActive, f.CreatedAt
    FROM Fundraiser f
    INNER JOIN Animal a ON f.AnimalId = a.AnimalId
    WHERE f.FundraiserId = @FundraiserId;
END
GO

-- RF-017: Catálogo público
CREATE OR ALTER PROCEDURE [dbo].[sp_ListarFundraisers]
AS
BEGIN
    SELECT f.FundraiserId, f.AnimalId, a.Nombre AS NombreAnimal,
           f.Titulo, f.Descripcion, f.MetaTotal, f.TotalActual,
           f.IsActive, f.CreatedAt
    FROM Fundraiser f
    INNER JOIN Animal a ON f.AnimalId = a.AnimalId
    WHERE f.IsActive = 1
    ORDER BY f.CreatedAt DESC;
END
GO

-- RF-017: Donaciones de un fundraiser (detalle)
CREATE OR ALTER PROCEDURE [dbo].[sp_ListarDonacionesPorFundraiser]
    @FundraiserId INT
AS
BEGIN
    SELECT d.DonacionId, d.FundraiserId, d.UsuarioId,
           ISNULL(u.PrimerNombre + ' ' + ISNULL(u.SegundoNombre + ' ', '') + u.PrimerApellido, 'AnÃ³nimo') AS NombreDonante,
           d.Total, d.DonatedAt
    FROM Donacion d
    LEFT JOIN Usuario u ON d.UsuarioId = u.UsuarioId
    WHERE d.FundraiserId = @FundraiserId
    ORDER BY d.DonatedAt DESC;
END
GO

-- RF-022: Historial del usuario (activos + inactivos)
CREATE OR ALTER PROCEDURE [dbo].[sp_ListarHistorialFundraisers]
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SELECT f.FundraiserId, f.AnimalId, a.Nombre AS NombreAnimal,
           f.Titulo, f.Descripcion, f.MetaTotal, f.TotalActual,
           f.IsActive, f.CreatedAt
    FROM Fundraiser f
    INNER JOIN Animal a ON f.AnimalId = a.AnimalId
    WHERE a.UsuarioId = @UsuarioId
    ORDER BY f.CreatedAt DESC;
END
GO

-- RF-020 + RF-030: Registrar donación y retornar datos para notificación
CREATE OR ALTER PROCEDURE [dbo].[sp_RegistrarDonacion]
    @FundraiserId INT,
    @UsuarioId NVARCHAR(450) = NULL,
    @Total DECIMAL(10,2)
AS
BEGIN
    INSERT INTO Donacion (FundraiserId, UsuarioId, Total)
    VALUES (@FundraiserId, @UsuarioId, @Total);

    UPDATE Fundraiser
    SET TotalActual = TotalActual + @Total
    WHERE FundraiserId = @FundraiserId;

    -- Datos para notificaciÃ³n RF-030
    SELECT u.CorreoElectronico,
           u.PrimerNombre,
           f.Titulo AS TituloFundraiser,
           f.TotalActual AS NuevoTotal,
           f.MetaTotal
    FROM Fundraiser f
    INNER JOIN Animal a ON f.AnimalId = a.AnimalId
    INNER JOIN Usuario u ON a.UsuarioId = u.UsuarioId
    WHERE f.FundraiserId = @FundraiserId;
END
GO

-- =============================================
-- 8. DATOS DE PRUEBA
-- =============================================

-- Usuario de prueba (pass: 12345678)
INSERT INTO Usuario (UsuarioId, CorreoElectronico, ContrasenaHash, PrimerNombre, PrimerApellido, IsActive, RolId)
VALUES ('usr-prueba-001', 'prueba@patitas.com', 'DkF5eJ1UhQwmEXbYNJDqmQ==', 'Usuario', 'Prueba', 1, 2);

--Usuario admin de prueba(pass: 12345678)
INSERT INTO Usuario (CorreoElectronico, ContrasenaHash, PrimerNombre, PrimerApellido, IsActive, RolId)
VALUES('admin@patitas.com','DkF5eJ1UhQwmEXbYNJDqmQ==','Dean','Winchester', 1, 1);

-- Animales de prueba
INSERT INTO Animal (UsuarioId, TipoId, Nombre, Peso, Edad, Sexo, Notas, CreatedBy)
VALUES
('usr-prueba-001', 1, 'Rocky', 12.00, '5 años',  'M', 'Necesita cirugía de cadera', 'usr-prueba-001'),
('usr-prueba-001', 2, 'Luna',  3.80,  '1 año',   'H', 'Muy cariñosa',              'usr-prueba-001'),
('usr-prueba-001', 1, 'Max',   8.50,  '3 años',  'M', 'Muy juguetón',              'usr-prueba-001');

-- Fundraisers de prueba
INSERT INTO Fundraiser (AnimalId, Titulo, Descripcion, MetaTotal, TotalActual, IsActive)
VALUES
(1, 'Ayuda para operación de Rocky', 'Rocky necesita una cirugí­a urgente de cadera. Cada colón cuenta para darle una mejor calidad de vida.', 150000, 45000, 1),
(2, 'Vacunas completas para Luna',   'Luna no ha recibido su esquema completo de vacunación. Ayudanos a mantenerla sana.', 50000, 10000, 1),
(3, 'Tratamiento dental de Max',     'Campaña finalizada. Gracias a todos los que donaron.', 80000, 80000, 0);

-- Donaciones de prueba
INSERT INTO Donacion (FundraiserId, UsuarioId, Total)
VALUES
(1, 'usr-prueba-001', 25000),
(1, NULL,             20000),
(2, 'usr-prueba-001', 10000);
GO 

-- =============================================
-- 9. SPs Mascotas (Mafeh)
-- =============================================
-- 23/3/2026 -----------Mascotas----------------------------
CREATE PROCEDURE [dbo].[sp_RegistrarPublicacionMascota]
    @UsuarioId NVARCHAR(450),
    @TipoId INT,
    @Nombre NVARCHAR(100),
    @Peso DECIMAL(5,2) = NULL,
    @Edad NVARCHAR(100) = NULL,
    @Sexo CHAR(1) = NULL,
    @Enfermedades NVARCHAR(MAX) = NULL,
    @HistorialMedico NVARCHAR(MAX) = NULL,
    @PreferenciasAlimenticias NVARCHAR(MAX) = NULL,
    @Notas NVARCHAR(MAX) = NULL,
    @Titulo NVARCHAR(200),
    @Descripcion NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AnimalId INT;
    DECLARE @PublicacionId INT;

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE UsuarioId = @UsuarioId AND IsActive = 1)
    BEGIN
        RAISERROR('El usuario no existe o está inactivo.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.AnimalTipo WHERE TipoId = @TipoId)
    BEGIN
        RAISERROR('El tipo de animal no existe.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Animal
    (
        UsuarioId,
        TipoId,
        Nombre,
        Peso,
        Edad,
        Sexo,
        Enfermedades,
        HistorialMedico,
        PreferenciasAlimenticias,
        Notas,
        CreatedBy
    )
    VALUES
    (
        @UsuarioId,
        @TipoId,
        @Nombre,
        @Peso,
        @Edad,
        @Sexo,
        @Enfermedades,
        @HistorialMedico,
        @PreferenciasAlimenticias,
        @Notas,
        @UsuarioId
    );

    SET @AnimalId = SCOPE_IDENTITY();

    INSERT INTO dbo.Publicacion
    (
        Titulo,
        Descripcion,
        AnimalId,
        IsActive,
        PublishedBy,
        UpdatedAt,
        UpdatedBy,
        ClosedAt
    )
    VALUES
    (
        @Titulo,
        @Descripcion,
        @AnimalId,
        1,
        @UsuarioId,
        NULL,
        NULL,
        NULL
    );

    SET @PublicacionId = SCOPE_IDENTITY();

    SELECT @PublicacionId AS PublicacionId, @AnimalId AS AnimalId;
END
GO


CREATE PROCEDURE [dbo].[sp_ListarMisPublicacionesMascota]
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PublicacionId,
        p.Titulo,
        p.Descripcion,
        p.IsActive,
        p.PublishedAt,
        p.UpdatedAt,
        p.ClosedAt,

        a.AnimalId,
        a.Nombre AS NombreMascota,
        a.Peso,
        a.Edad,
        a.Sexo,
        a.Enfermedades,
        a.HistorialMedico,
        a.PreferenciasAlimenticias,
        a.Notas,

        t.TipoId,
        t.NombreTipo AS TipoAnimal,
        c.CategoriaId,
        c.NombreTipo AS CategoriaAnimal

    FROM dbo.Publicacion p
    INNER JOIN dbo.Animal a ON p.AnimalId = a.AnimalId
    INNER JOIN dbo.AnimalTipo t ON a.TipoId = t.TipoId
    INNER JOIN dbo.AnimalCategoria c ON t.CategoriaId = c.CategoriaId
    WHERE p.PublishedBy = @UsuarioId
    ORDER BY p.PublishedAt DESC;
END
GO

CREATE PROCEDURE [dbo].[sp_ObtenerPublicacionMascota]
    @PublicacionId INT,
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PublicacionId,
        p.Titulo,
        p.Descripcion,
        p.IsActive,
        p.PublishedAt,
        p.UpdatedAt,
        p.ClosedAt,

        a.AnimalId,
        a.UsuarioId,
        a.TipoId,
        a.Nombre AS NombreMascota,
        a.Peso,
        a.Edad,
        a.Sexo,
        a.Enfermedades,
        a.HistorialMedico,
        a.PreferenciasAlimenticias,
        a.Notas,

        t.NombreTipo AS TipoAnimal,
        c.CategoriaId,
        c.NombreTipo AS CategoriaAnimal

    FROM dbo.Publicacion p
    INNER JOIN dbo.Animal a ON p.AnimalId = a.AnimalId
    INNER JOIN dbo.AnimalTipo t ON a.TipoId = t.TipoId
    INNER JOIN dbo.AnimalCategoria c ON t.CategoriaId = c.CategoriaId
    WHERE p.PublicacionId = @PublicacionId
      AND p.PublishedBy = @UsuarioId;
END
GO


-- 24/3/2026 ----------- Editar Mascotas----------------------------
CREATE PROCEDURE dbo.sp_EditarPublicacionMascota
    @PublicacionId INT,
    @UsuarioId NVARCHAR(450),
    @Titulo NVARCHAR(200),
    @Descripcion NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Publicacion
    SET 
        Titulo = @Titulo,
        Descripcion = @Descripcion,
        UpdatedAt = GETDATE(),
        UpdatedBy = @UsuarioId
    WHERE PublicacionId = @PublicacionId
      AND PublishedBy = @UsuarioId
      AND IsActive = 1;

    IF @@ROWCOUNT > 0
        SELECT 1 AS Resultado;
    ELSE
        SELECT 0 AS Resultado;
END
GO

-- 24/3/2026 ----------- Inactivar Mascotas----------------------------
CREATE PROCEDURE dbo.sp_InactivarPublicacionMascota
    @PublicacionId INT,
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Publicacion
    SET 
        IsActive = 0,
        ClosedAt = GETDATE(),
        UpdatedAt = GETDATE(),
        UpdatedBy = @UsuarioId
    WHERE PublicacionId = @PublicacionId
      AND PublishedBy = @UsuarioId
      AND IsActive = 1;

    IF @@ROWCOUNT > 0
        SELECT 1 AS Resultado;
    ELSE
        SELECT 0 AS Resultado;
END
GO

CREATE PROCEDURE [dbo].[sp_RegistrarAnimalMedia]
    @AnimalId INT,
    @ArchivoUrl VARCHAR(MAX),
    @FileSize BIGINT,
    @CreatedBy NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Usuario
        WHERE UsuarioId = @CreatedBy
          AND IsActive = 1
    )
    BEGIN
        RAISERROR('El usuario no existe o está inactivo.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Animal
        WHERE AnimalId = @AnimalId
    )
    BEGIN
        RAISERROR('El animal no existe.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.AnimalMedia
    (
        AnimalId,
        ArchivoUrl,
        FileSize,
        CreatedBy
    )
    VALUES
    (
        @AnimalId,
        @ArchivoUrl,
        @FileSize,
        @CreatedBy
    );

    SELECT SCOPE_IDENTITY() AS MediaId;
END
GO

CREATE PROCEDURE [dbo].[sp_ListarAnimalMedia]
    @AnimalId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MediaId,
        AnimalId,
        ArchivoUrl,
        FileSize,
        CreatedAt,
        CreatedBy
    FROM dbo.AnimalMedia
    WHERE AnimalId = @AnimalId
    ORDER BY CreatedAt DESC;
END
GO

CREATE PROCEDURE [dbo].[sp_EliminarAnimalMedia]
    @MediaId INT,
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.AnimalMedia
        WHERE MediaId = @MediaId
          AND CreatedBy = @UsuarioId
    )
    BEGIN
        RAISERROR('La imagen no existe o no pertenece al usuario.', 16, 1);
        RETURN;
    END

    DELETE FROM dbo.AnimalMedia
    WHERE MediaId = @MediaId;
END
GO

CREATE PROCEDURE [dbo].[sp_ListarTiposAnimal]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        TipoId,
        NombreTipo
    FROM dbo.AnimalTipo
    ORDER BY NombreTipo ASC
END
GO
