USE hopbd;
GO

-- Tabla de Roles
CREATE TABLE Roles (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL
);

INSERT INTO Roles (nombre) VALUES ('Administrador'), ('Usuario');

-- Tabla de Usuarios (simplificada para tu backend)
CREATE TABLE Usuarios (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    rol INT DEFAULT 2,
    Email VARCHAR(100) UNIQUE NOT NULL,
    FechaRegistro DATETIME2 DEFAULT GETDATE(),
    Activo BIT DEFAULT 1,
    NombreCompleto VARCHAR(150),
    Bio TEXT,
    FotoURL VARCHAR(500),
    Telefono VARCHAR(20)
);

-- Tabla de Categorias
CREATE TABLE Categorias (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL
);

INSERT INTO Categorias (nombre) VALUES 
    ('Construcción'), ('Plomería'), ('Electricidad'), 
    ('Jardinería'), ('Pintura'), ('Mecánica'), 
    ('Limpieza'), ('Otro');

-- Tabla de Servicios
CREATE TABLE Servicios (
    id INT IDENTITY(1,1) PRIMARY KEY,
    titulo VARCHAR(200) NOT NULL,
    usuarioId INT NOT NULL,
    ubicacion VARCHAR(100),
    fechaRegistro DATETIME2 DEFAULT GETDATE(),
    categoriaId INT,
    descripcion TEXT,
    estado VARCHAR(20) DEFAULT 'activo'
);

-- Tabla de Postulaciones
CREATE TABLE Postulaciones (
    id INT IDENTITY(1,1) PRIMARY KEY,
    servicioId INT NOT NULL,
    prestadorId INT NOT NULL,
    fecha DATETIME2 DEFAULT GETDATE(),
    estado VARCHAR(20) DEFAULT 'pendiente'
);

-- STORED PROCEDURES (los que usa tu backend)

CREATE OR ALTER PROCEDURE Login
    @Email VARCHAR(100),
    @Password VARCHAR(100)
AS
BEGIN
    SELECT 
        u.id,
        u.name,
        r.nombre AS Rol,
        u.NombreCompleto
    FROM Usuarios u
    INNER JOIN Roles r ON u.rol = r.id
    WHERE u.Email = @Email AND u.Activo = 1
END;
GO

CREATE OR ALTER PROCEDURE RegistrarUsuario
    @Nombre VARCHAR(100),
    @RolID INT,
    @Email VARCHAR(100),
    @Password VARCHAR(100)
AS
BEGIN
    INSERT INTO Usuarios (name, rol, Email, NombreCompleto)
    VALUES (@Nombre, @RolID, @Email, @Nombre)
    
    SELECT SCOPE_IDENTITY() AS id
END;
GO

CREATE OR ALTER PROCEDURE ObtenerServicios
    @Busqueda VARCHAR(200) = NULL,
    @CategoriaID INT = NULL
AS
BEGIN
    SELECT 
        s.id,
        s.titulo,
        s.descripcion,
        s.ubicacion,
        c.nombre AS Categoria,
        u.name AS Autor,
        s.fechaRegistro
    FROM Servicios s
    LEFT JOIN Categorias c ON s.categoriaId = c.id
    LEFT JOIN Usuarios u ON s.usuarioId = u.id
    WHERE s.estado = 'activo'
        AND (@Busqueda IS NULL OR s.titulo LIKE '%' + @Busqueda + '%')
        AND (@CategoriaID IS NULL OR s.categoriaId = @CategoriaID)
    ORDER BY s.fechaRegistro DESC
END;
GO

CREATE OR ALTER PROCEDURE CrearServicio
    @Titulo VARCHAR(200),
    @UsuarioID INT,
    @Ubicacion VARCHAR(100),
    @CategoriaID INT,
    @Descripcion TEXT
AS
BEGIN
    INSERT INTO Servicios (titulo, usuarioId, ubicacion, categoriaId, descripcion)
    VALUES (@Titulo, @UsuarioID, @Ubicacion, @CategoriaID, @Descripcion)
    
    SELECT SCOPE_IDENTITY() AS id
END;
GO

CREATE OR ALTER PROCEDURE CrearPostulacion
    @ServicioID INT,
    @PrestadorID INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Postulaciones WHERE servicioId = @ServicioID AND prestadorId = @PrestadorID)
    BEGIN
        RAISERROR('Ya te has postulado a este servicio', 16, 1)
        RETURN
    END
    
    INSERT INTO Postulaciones (servicioId, prestadorId)
    VALUES (@ServicioID, @PrestadorID)
    
    SELECT SCOPE_IDENTITY() AS id
END;
GO

CREATE OR ALTER PROCEDURE ActualizarPerfil
    @UsuarioID INT,
    @NombreCompleto VARCHAR(150),
    @Bio TEXT,
    @Foto VARCHAR(500),
    @Tel VARCHAR(20)
AS
BEGIN
    UPDATE Usuarios 
    SET NombreCompleto = @NombreCompleto,
        Bio = @Bio,
        FotoURL = @Foto,
        Telefono = @Tel
    WHERE id = @UsuarioID
END;
GO

-- DATOS DE PRUEBA
INSERT INTO Usuarios (name, rol, Email, NombreCompleto, Activo)
SELECT 'Admin', 1, 'admin@hop.com', 'Administrador HOP', 1
WHERE NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'admin@hop.com');

INSERT INTO Usuarios (name, rol, Email, NombreCompleto, Activo)
SELECT 'Juan Perez', 2, 'juan@test.com', 'Juan Pérez García', 1
WHERE NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'juan@test.com');

INSERT INTO Servicios (titulo, usuarioId, ubicacion, categoriaId, descripcion)
SELECT 'Construcción de Casa', 2, 'Los Mochis', 1, 'Necesito construir una casa de 2 pisos'
WHERE NOT EXISTS (SELECT 1 FROM Servicios WHERE titulo = 'Construcción de Casa');