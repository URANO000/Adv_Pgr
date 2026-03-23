DROP DATABASE patitassocial
-- 11.02.2026::6:30 PM --
CREATE DATABASE patitassocial;
USE patitassocial;
GO

--Creación de tablas y junction tables--
--Roles & Usuarios--
CREATE TABLE Rol (
	RolId INT PRIMARY KEY IDENTITY (1,1),
	NombreRol NVARCHAR(100) NOT NULL
);


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
	Provincia NVARCHAR(100) null,
	CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
	IsActive BIT NOT NULL,
	RolId INT NOT NULL,
	ImagenPerfil VARCHAR(MAX) NULL,

	CONSTRAINT [FK_Usuario_Rol_RolId] FOREIGN KEY (RolId)
	REFERENCES Rol(RolId),
);

ALTER TABLE Usuario
ADD CONSTRAINT UQ_UsuarioId UNIQUE (UsuarioId);

--Animales, Tipos
--Caninos, felinos, conejos, roedores, aves, etc....
CREATE TABLE AnimalCategoria(
	CategoriaId INT PRIMARY KEY IDENTITY(1,1),
	NombreTipo NVARCHAR(100)
);

--Perros, gatos, conejos, ratones, chinchillas, periquitos, etc.....
CREATE TABLE AnimalTipo(
	TipoId INT PRIMARY KEY IDENTITY(1,1),
	NombreTipo NVARCHAR(100),
	CategoriaId INT,

	CONSTRAINT [FK_AnimalTipo_AnimalCategoria_CategoriaId] FOREIGN KEY (CategoriaId)
	REFERENCES AnimalCategoria (CategoriaId)
);

CREATE TABLE Animal(
	AnimalId INT IDENTITY PRIMARY KEY,
	UsuarioId  NVARCHAR(450) NOT NULL,  --Es el dueño actual
	TipoId INT NOT NULL,

	Nombre NVARCHAR(100) NOT NULL,
	Peso DECIMAL (5,2) NULL,
	Edad NVARCHAR(100) NULL,
	Sexo CHAR(1) NULL CHECK (Sexo IN('M', 'H')),
	
	Enfermedades NVARCHAR(MAX) NULL,
	HistorialMedico NVARCHAR(MAX) NULL,
	PreferenciasAlimenticias NVARCHAR(MAX) NUll,
	Notas NVARCHAR(MAX) NULL,

	CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
	CreatedBy NVARCHAR(450),
	
	CONSTRAINT [FK_Animal_Usuario_CreatedBy] FOREIGN KEY(CreatedBy)
	REFERENCES Usuario(UsuarioId),
	CONSTRAINT [FK_Animal_Tipo_TipoId] FOREIGN KEY (TipoId)
	REFERENCES AnimalTipo(TipoId)
);


--Media (Videos, fotos) guarda archivo en url, etc--
CREATE TABLE AnimalMedia (
	MediaId INT IDENTITY PRIMARY KEY NOT NULL,
	AnimalId INT NOT NULL,
	ArchivoUrl VARCHAR(MAX),
	FileSize BIGINT NOT NULL,
	CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
	CreatedBy NVARCHAR(450),

	CONSTRAINT [FK_AnimalMedia_Animal_AnimalId] FOREIGN KEY(AnimalId)
	REFERENCES Animal(AnimalId),
	CONSTRAINT [FK_AnimalMedia_Usuario_CreatedBy] FOREIGN KEY(CreatedBy)
	REFERENCES Usuario(UsuarioId)
);

--Publicación, es necesaria por cosas como Título, Descripción
CREATE TABLE Publicacion(
	PublicacionId INT IDENTITY PRIMARY KEY NOT NULL,
	Titulo NVARCHAR(200),
	Descripcion NVARCHAR(300),
	AnimalId INT NOT NULL,
	IsActive BIT,
	PublishedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
	PublishedBy NVARCHAR(450),
	UpdatedAt DATETIME2 DEFAULT SYSDATETIME(),
	UpdatedBy NVARCHAR(450),
	ClosedAt DATETIME2 DEFAULT SYSDATETIME(),

	CONSTRAINT [FK_Publicacion_Animal_AnimalId] FOREIGN KEY (AnimalId)
	REFERENCES Animal (AnimalId),
	CONSTRAINT [FK_Publicacion_Usuario_PublishedBy] FOREIGN KEY (PublishedBy)
	REFERENCES Usuario (UsuarioId),
	CONSTRAINT [FK_Publicacion_Usuario_UpdatedBy] FOREIGN KEY (UpdatedBy)
	REFERENCES Usuario (UsuarioId)
);

--Solicitudes--
CREATE TABLE Solicitud(
	Id INT IDENTITY PRIMARY KEY,
	PublicacionId INT NOT NULL,
	UsuarioInteresadoId NVARCHAR(450) NOT NULL,

	Mensaje NVARCHAR(MAX) NOT NULL,
	SentAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

	CONSTRAINT [FK_Solicitud_Publicacion_PublicacionId] FOREIGN KEY(PublicacionId)
	REFERENCES Publicacion(PublicacionId),
	CONSTRAINT [FK_Solicitud_Usuario_UsuarioInteresadoId] FOREIGN KEY(UsuarioInteresadoId)
	REFERENCES Usuario(UsuarioId)
);

--Fundraiser y donaciones
--La tabla fundraiser es la publicación por así decirlo
CREATE TABLE Fundraiser (
	FundraiserId INT IDENTITY PRIMARY KEY,
	AnimalId INT NOT NULL,
	Titulo NVARCHAR(200) NOT NULL,
	Descripcion NVARCHAR(MAX) NOT NULL,
	MetaTotal DECIMAL(10,2) NOT NULL,
	TotalActual DECIMAL(10,2) NOT NULL DEFAULT 0,
	IsActive BIT NOT NULL,
	CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()

	CONSTRAINT [FK_Fundraiser_Animal_AnimalId] FOREIGN KEY(AnimalId)
	REFERENCES Animal(AnimalId)
);

--La tabla donación es especificamente las donaciones por usuarios
CREATE TABLE Donacion(
	DonacionId INT IDENTITY PRIMARY KEY,
	FundraiserId INT NOT NULL,
	UsuarioId NVARCHAR(450) NULL,
	Total DECIMAL(10,2) NOT NULL,
	DonatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

	CONSTRAINT [FK_Donacion_Fundraiser_FundraiserId] FOREIGN KEY(FundraiserId)
	REFERENCES Fundraiser(FundraiserId),
	CONSTRAINT [FK_Donacion_Usuario_UsuarioId] FOREIGN KEY(UsuarioId)
	REFERENCES Usuario(UsuarioId)
);

-- =============================================
-- Datos de prueba
-- =============================================

-- Categorías
INSERT INTO dbo.AnimalCategoria (NombreTipo) VALUES
('Caninos'),
('Felinos'),
('Roedores'),
('Aves');
GO

-- Tipos
INSERT INTO dbo.AnimalTipo (NombreTipo, CategoriaId) VALUES
('Perro',       1),
('Gato',        2),
('Conejo',      3),
('Hamster',     3),
('Periquito',   4);
GO

-- Animales (UsuarioId y CreatedBy en NULL por ahora, sin autenticación)
INSERT INTO dbo.Animal (UsuarioId, TipoId, Nombre, Peso, Edad, Sexo, Notas, CreatedBy) VALUES
('1', 1, 'Luna',   8.50,  '3 años',   'H', 'Muy juguetona',      NULL),
('1', 1, 'Rocky',  12.00, '5 años',   'M', 'Necesita cirugía',   NULL),
('1', 2, 'Milo',   4.20,  '2 años',   'M', 'Alérgico al polen',  NULL),
('1', 2, 'Nala',   3.80,  '1 año',    'H', 'Muy cariñosa',       NULL),
('1', 3, 'Toto',   1.20,  '8 meses',  'M', 'Le gusta la lechuga',NULL);
GO

--Rol Id --19/3/2026
INSERT INTO dbo.Rol(NombreRol)
VALUES ('Administrador'), ('Usuario Normal');

--Usuario admin de prueba -- 23/3/2026
--Password es 12345678
INSERT INTO dbo.Usuario(CorreoElectronico, ContrasenaHash, PrimerNombre, PrimerApellido, Telefono, Provincia, RolId, IsActive)
VALUES ('admin@gmail.com', 'DkF5eJ1UhQwmEXbYNJDqmQ==', 'Dean', 'Winchester', '8878-1949', 'Heredia', 1, 1 );

/* PROCESOS ALMACENADOS */

CREATE PROCEDURE [dbo].[sp_ListarAnimales]
AS
BEGIN

    SELECT  a.AnimalId,
            a.Nombre + ' (' + t.NombreTipo + ')' AS Nombre
    FROM    dbo.Animal      a
    JOIN    dbo.AnimalTipo  t ON t.TipoId = a.TipoId
    ORDER BY a.Nombre ASC

END
GO

CREATE PROCEDURE [dbo].[sp_RegistrarFundraiser]
    @AnimalId       INT,
    @Titulo         NVARCHAR(200),
    @Descripcion    NVARCHAR(MAX),
    @MetaTotal      DECIMAL(10,2)
AS
BEGIN

    IF EXISTS (SELECT 1 FROM dbo.Animal WHERE AnimalId = @AnimalId)
    BEGIN

        INSERT INTO dbo.Fundraiser (AnimalId, Titulo, Descripcion, MetaTotal, TotalActual, IsActive)
        VALUES (@AnimalId, @Titulo, @Descripcion, @MetaTotal, 0, 1)

    END

END
GO

CREATE PROCEDURE [dbo].[sp_EditarFundraiser]
    @FundraiserId   INT,
    @AnimalId       INT,
    @Titulo         NVARCHAR(200),
    @Descripcion    NVARCHAR(MAX),
    @MetaTotal      DECIMAL(10,2)
AS
BEGIN
 
    IF EXISTS (SELECT 1 FROM dbo.Fundraiser WHERE FundraiserId = @FundraiserId AND IsActive = 1)
    BEGIN
 
        UPDATE  dbo.Fundraiser
        SET     AnimalId    = @AnimalId,
                Titulo      = @Titulo,
                Descripcion = @Descripcion,
                MetaTotal   = @MetaTotal
        WHERE   FundraiserId = @FundraiserId
 
    END
 
END
GO

CREATE PROCEDURE [dbo].[sp_ObtenerFundraiser]
    @FundraiserId INT
AS
BEGIN

    SELECT  FundraiserId,
            AnimalId,
            Titulo,
            Descripcion,
            MetaTotal,
            TotalActual,
            IsActive,
            CreatedAt
    FROM    dbo.Fundraiser
    WHERE   FundraiserId = @FundraiserId

END
GO

CREATE PROCEDURE [dbo].[sp_InactivarFundraiser]
    @FundraiserId INT
AS
BEGIN

    IF EXISTS (SELECT 1 FROM dbo.Fundraiser WHERE FundraiserId = @FundraiserId AND IsActive = 1)
    BEGIN

        UPDATE  dbo.Fundraiser
        SET     IsActive = 0
        WHERE   FundraiserId = @FundraiserId

    END

END
GO


-- 19/3/2026 -----------Autenticación----------------------------
CREATE PROCEDURE [dbo].[sp_RegistrarCuenta]
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


CREATE PROCEDURE [dbo].[sp_ActualizarContrasenna]
	@UsuarioId NVARCHAR(450),
	@Contrasenna NVARCHAR(500)
AS
BEGIN
	UPDATE [dbo].[Usuario]
	SET ContrasenaHash = @Contrasenna
	WHERE UsuarioId = @UsuarioId
END
GO

CREATE PROCEDURE [dbo].[sp_ValidarCorreo]
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

CREATE PROCEDURE [dbo].[sp_ObtenerUsuario]
	@UsuarioId NVARCHAR(450)
AS
BEGIN
	SELECT 
		u.CorreoElectronico,
		u.PrimerNombre,
		u.SegundoNombre,
		u.PrimerApellido,
		u.SegundoApellido,
		u.Cedula,
		u.Telefono,
		u.Provincia,
		u.ImagenPerfil
	FROM Usuario u
	WHERE u.UsuarioId = @UsuarioId
	AND u.IsActive = 1
END
GO

CREATE PROCEDURE [dbo].[sp_ObtenerUsuarios]
AS
BEGIN
	SELECT CorreoElectronico, PrimerNombre, SegundoNombre,
	PrimerApellido, SegundoApellido, Cedula, Telefono, Provincia, CreatedAt,
	RolId, ImagenPerfil
	FROM Usuario
	ORDER BY CreatedAt 
END
GO


CREATE PROCEDURE [dbo].[sp_EditarUsuario]
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

CREATE PROCEDURE [dbo].[sp_DesactivarUsuario]
	@UsuarioId NVARCHAR(450)
AS
BEGIN
	UPDATE [dbo].[Usuario]
	SET IsActive = 0
	WHERE UsuarioId = @UsuarioId
END
GO