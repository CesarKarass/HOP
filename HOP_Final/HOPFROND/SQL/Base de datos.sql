57-- Crear la base de datos
CREATE DATABASE hopbd
GO

USE hopbd;
GO

-- ============================================
-- TABLAS
-- ============================================

-- Tabla de Roles (según tu código, usas RolID)
CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL,
    Descripcion NVARCHAR(200)
);
GO

-- Tabla de Usuarios
CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    NombreCompleto NVARCHAR(200) NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,  -- En producción usar hash
    RolID INT FOREIGN KEY REFERENCES Roles(Id) DEFAULT 2,
    Telefono NVARCHAR(20) NULL,
    Ubicacion NVARCHAR(100) NULL,
    Bio NVARCHAR(500) NULL,
    FotoURL NVARCHAR(500) NULL,
    FechaRegistro DATETIME DEFAULT GETDATE(),
    Activo BIT DEFAULT 1
);
GO

-- Tabla de Categorías de Servicios
CREATE TABLE Categorias (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500),
    TipoOficio NVARCHAR(20) CHECK (TipoOficio IN ('formal', 'informal', 'ambos')) DEFAULT 'ambos'
);
GO

-- Tabla de Servicios
CREATE TABLE Servicios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Titulo NVARCHAR(200) NOT NULL,
    UsuarioID INT FOREIGN KEY REFERENCES Usuarios(Id),
    Ubicacion NVARCHAR(100) NOT NULL,
    CategoriaID INT FOREIGN KEY REFERENCES Categorias(Id),
    Descripcion NVARCHAR(MAX),
    Pago DECIMAL(10,2) NULL,
    TipoPago NVARCHAR(20) CHECK (TipoPago IN ('por_hora', 'por_dia', 'por_proyecto', 'negociable')),
    Urgencia NVARCHAR(20) CHECK (Urgencia IN ('inmediata', 'urgente', 'normal', 'flexible')),
    Estado NVARCHAR(20) DEFAULT 'activo' CHECK (Estado IN ('activo', 'completado', 'cancelado', 'pausado')),
    FechaRegistro DATETIME DEFAULT GETDATE(),
    FechaExpiracion DATETIME NULL,
    Visitas INT DEFAULT 0
);
GO

-- Tabla de Conversaciones (para chat)
CREATE TABLE Conversaciones (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Usuario1ID INT FOREIGN KEY REFERENCES Usuarios(Id),
    Usuario2ID INT FOREIGN KEY REFERENCES Usuarios(Id),
    FechaInicio DATETIME DEFAULT GETDATE(),
    UltimoMensaje DATETIME NULL
);
GO

-- Tabla de Mensajes
CREATE TABLE Mensajes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ConversacionID INT FOREIGN KEY REFERENCES Conversaciones(Id),
    EmisorID INT FOREIGN KEY REFERENCES Usuarios(Id),
    ReceptorID INT FOREIGN KEY REFERENCES Usuarios(Id),
    Mensaje NVARCHAR(MAX) NOT NULL,
    FechaEnvio DATETIME DEFAULT GETDATE(),
    Leido BIT DEFAULT 0,
    FechaLeido DATETIME NULL
);
GO

-- Tabla de Postulaciones (aplicaciones a servicios)
CREATE TABLE Postulaciones (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ServicioID INT FOREIGN KEY REFERENCES Servicios(Id),
    PrestadorID INT FOREIGN KEY REFERENCES Usuarios(Id),
    Estado NVARCHAR(20) DEFAULT 'pendiente' CHECK (Estado IN ('pendiente', 'aceptado', 'rechazado', 'cancelado')),
    Mensaje NVARCHAR(500) NULL,
    FechaPostulacion DATETIME DEFAULT GETDATE(),
    FechaRespuesta DATETIME NULL,
    CONSTRAINT UQ_Postulacion UNIQUE (ServicioID, PrestadorID)
);
GO

-- Tabla de Notificaciones
CREATE TABLE Notificaciones (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT FOREIGN KEY REFERENCES Usuarios(Id),
    Tipo NVARCHAR(50) CHECK (Tipo IN ('mensaje', 'postulacion', 'servicio', 'sistema')),
    Contenido NVARCHAR(500) NOT NULL,
    ReferenciaID INT NULL,
    Leida BIT DEFAULT 0,
    FechaCreacion DATETIME DEFAULT GETDATE()
);
GO

-- Tabla de Calificaciones
CREATE TABLE Calificaciones (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CalificadorID INT FOREIGN KEY REFERENCES Usuarios(Id),
    CalificadoID INT FOREIGN KEY REFERENCES Usuarios(Id),
    ServicioID INT FOREIGN KEY REFERENCES Servicios(Id),
    Puntuacion INT CHECK (Puntuacion BETWEEN 1 AND 5),
    Comentario NVARCHAR(500),
    Fecha DATETIME DEFAULT GETDATE()
);
GO

-- Tabla de Reportes
CREATE TABLE Reportes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ReportanteID INT FOREIGN KEY REFERENCES Usuarios(Id),
    ReportadoID INT FOREIGN KEY REFERENCES Usuarios(Id),
    ServicioID INT FOREIGN KEY REFERENCES Servicios(Id) NULL,
    Motivo NVARCHAR(255),
    Descripcion NVARCHAR(MAX),
    Estado NVARCHAR(20) DEFAULT 'pendiente' CHECK (Estado IN ('pendiente', 'revisado', 'resuelto', 'rechazado')),
    FechaReporte DATETIME DEFAULT GETDATE()
);
GO

-- ============================================
-- DATOS INICIALES (INSERTS)
-- ============================================

-- Insertar Roles
INSERT INTO Roles (Nombre, Descripcion) VALUES
('admin', 'Administrador del sistema'),
('trabajador', 'Usuario que ofrece servicios'),
('contratante', 'Usuario que busca contratar servicios'),
('ambos', 'Usuario que ofrece y contrata servicios');
GO

-- Insertar Categorías de Servicios
INSERT INTO Categorias (Nombre, Descripcion, TipoOficio) VALUES
('Construcción', 'Albañilería, remodelaciones, mantenimiento', 'ambos'),
('Plomería', 'Reparaciones de fontanería, instalaciones', 'informal'),
('Electricidad', 'Instalaciones eléctricas, reparaciones', 'ambos'),
('Jardinería', 'Mantenimiento de jardines, poda', 'informal'),
('Pintura', 'Pintura interior y exterior', 'informal'),
('Mecánica', 'Reparación de vehículos', 'formal'),
('Carpintería', 'Muebles a medida, reparaciones', 'informal'),
('Limpieza', 'Servicios de limpieza general', 'informal'),
('Mudanzas', 'Transporte de muebles y mudanzas', 'informal'),
('Cocina', 'Preparación de alimentos a domicilio', 'informal'),
('Fotografía', 'Fotografía para eventos', 'formal'),
('Clases', 'Clases particulares y tutorías', 'ambos'),
('Tecnología', 'Reparación de computadoras, programación', 'formal'),
('Cuidado Personal', 'Peluquería, maquillaje, estética', 'informal'),
('Cuidado de Adultos', 'Cuidado de personas mayores', 'ambos');
GO

-- ============================================
-- STORED PROCEDURES
-- ============================================

-- Registrar Usuario
CREATE OR ALTER PROCEDURE RegistrarUsuario
    @Nombre NVARCHAR(100),
    @RolID INT,
    @Email NVARCHAR(100),
    @Password NVARCHAR(255)
AS
BEGIN
    INSERT INTO Usuarios (Nombre, Email, Password, RolID, NombreCompleto)
    VALUES (@Nombre, @Email, @Password, @RolID, @Nombre);
    
    SELECT SCOPE_IDENTITY() AS Id;
END
GO

-- Login
CREATE OR ALTER PROCEDURE Login
    @Email NVARCHAR(100),
    @Password NVARCHAR(255)
AS
BEGIN
    SELECT 
        u.Id,
        u.Nombre AS name,
        r.Nombre AS Rol,
        u.NombreCompleto
    FROM Usuarios u
    INNER JOIN Roles r ON u.RolID = r.Id
    WHERE u.Email = @Email AND u.Password = @Password AND u.Activo = 1
END
GO

-- Actualizar Perfil
CREATE OR ALTER PROCEDURE ActualizarPerfil
    @UsuarioID INT,
    @NombreCompleto NVARCHAR(200),
    @Bio NVARCHAR(500),
    @Foto NVARCHAR(500),
    @Tel NVARCHAR(20)
AS
BEGIN
    UPDATE Usuarios 
    SET NombreCompleto = @NombreCompleto,
        Bio = @Bio,
        FotoURL = @Foto,
        Telefono = @Tel
    WHERE Id = @UsuarioID
END
GO

-- Crear Servicio
CREATE OR ALTER PROCEDURE CrearServicio
    @Titulo NVARCHAR(200),
    @UsuarioID INT,
    @Ubicacion NVARCHAR(100),
    @CategoriaID INT,
    @Descripcion NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO Servicios (Titulo, UsuarioID, Ubicacion, CategoriaID, Descripcion)
    VALUES (@Titulo, @UsuarioID, @Ubicacion, @CategoriaID, @Descripcion)
    
    SELECT SCOPE_IDENTITY() AS Id
END
GO

-- Obtener Servicios (con filtros)
CREATE OR ALTER PROCEDURE ObtenerServicios
    @Busqueda NVARCHAR(200) = NULL,
    @CategoriaID INT = NULL,
    @Ubicacion NVARCHAR(100) = NULL
AS
BEGIN
    SELECT 
        s.Id,
        s.Titulo,
        s.Ubicacion,
        c.Nombre AS Categoria,
        u.Nombre AS Autor,
        s.FechaRegistro
    FROM Servicios s
    INNER JOIN Categorias c ON s.CategoriaID = c.Id
    INNER JOIN Usuarios u ON s.UsuarioID = u.Id
    WHERE s.Estado = 'activo'
        AND (@Busqueda IS NULL OR s.Titulo LIKE '%' + @Busqueda + '%' OR s.Descripcion LIKE '%' + @Busqueda + '%')
        AND (@CategoriaID IS NULL OR s.CategoriaID = @CategoriaID)
        AND (@Ubicacion IS NULL OR s.Ubicacion LIKE '%' + @Ubicacion + '%')
    ORDER BY s.FechaRegistro DESC
END
GO

-- Crear Postulación
CREATE OR ALTER PROCEDURE CrearPostulacion
    @ServicioID INT,
    @PrestadorID INT
AS
BEGIN
    -- Verificar si ya existe postulación
    IF EXISTS (SELECT 1 FROM Postulaciones WHERE ServicioID = @ServicioID AND PrestadorID = @PrestadorID)
    BEGIN
        RAISERROR('Ya te has postulado a este servicio', 16, 1)
        RETURN
    END
    
    INSERT INTO Postulaciones (ServicioID, PrestadorID)
    VALUES (@ServicioID, @PrestadorID)
    
    -- Crear notificación para el dueño del servicio
    DECLARE @UsuarioID INT
    SELECT @UsuarioID = UsuarioID FROM Servicios WHERE Id = @ServicioID
    
    INSERT INTO Notificaciones (UsuarioID, Tipo, Contenido, ReferenciaID)
    VALUES (@UsuarioID, 'postulacion', 'Un usuario se ha postulado a tu servicio', @ServicioID)
END
GO

-- Obtener Postulaciones por Servicio
CREATE OR ALTER PROCEDURE ObtenerPostulacionesPorServicio
    @ServicioID INT
AS
BEGIN
    SELECT 
        p.Id AS PostulacionID,
        p.FechaPostulacion AS fecha,
        p.Estado,
        u.Nombre AS UsuarioNombre,
        u.NombreCompleto,
        u.Telefono,
        u.FotoURL
    FROM Postulaciones p
    INNER JOIN Usuarios u ON p.PrestadorID = u.Id
    WHERE p.ServicioID = @ServicioID
    ORDER BY p.FechaPostulacion DESC
END
GO

-- Obtener o Crear Conversación
CREATE OR ALTER PROCEDURE ObtenerConversacion
    @User1 INT,
    @User2 INT
AS
BEGIN
    -- Buscar conversación existente
    DECLARE @ConvId INT
    
    SELECT @ConvId = Id 
    FROM Conversaciones 
    WHERE (Usuario1ID = @User1 AND Usuario2ID = @User2)
       OR (Usuario1ID = @User2 AND Usuario2ID = @User1)
    
    -- Si no existe, crear una nueva
    IF @ConvId IS NULL
    BEGIN
        INSERT INTO Conversaciones (Usuario1ID, Usuario2ID)
        VALUES (@User1, @User2)
        
        SET @ConvId = SCOPE_IDENTITY()
    END
    
    SELECT @ConvId AS id
END
GO

-- Enviar Mensaje
CREATE OR ALTER PROCEDURE EnviarMensaje
    @ConversacionID INT,
    @EmisorID INT,
    @ReceptorID INT,
    @Mensaje NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO Mensajes (ConversacionID, EmisorID, ReceptorID, Mensaje)
    VALUES (@ConversacionID, @EmisorID, @ReceptorID, @Mensaje)
    
    -- Actualizar último mensaje en conversación
    UPDATE Conversaciones 
    SET UltimoMensaje = GETDATE() 
    WHERE Id = @ConversacionID
    
    -- Crear notificación para el receptor
    INSERT INTO Notificaciones (UsuarioID, Tipo, Contenido, ReferenciaID)
    VALUES (@ReceptorID, 'mensaje', 'Tienes un nuevo mensaje', @ConversacionID)
END
GO

-- Obtener Notificaciones
CREATE OR ALTER PROCEDURE ObtenerNotificaciones
    @UsuarioID INT
AS
BEGIN
    SELECT 
        Id,
        Contenido,
        Tipo,
        Leida,
        FechaCreacion
    FROM Notificaciones
    WHERE UsuarioID = @UsuarioID
    ORDER BY FechaCreacion DESC
END
GO

-- ============================================
-- ÍNDICES PARA OPTIMIZACIÓN
-- ============================================

CREATE INDEX IX_Usuarios_Email ON Usuarios(Email);
CREATE INDEX IX_Usuarios_RolID ON Usuarios(RolID);
CREATE INDEX IX_Servicios_UsuarioID ON Servicios(UsuarioID);
CREATE INDEX IX_Servicios_CategoriaID ON Servicios(CategoriaID);
CREATE INDEX IX_Servicios_Estado ON Servicios(Estado);
CREATE INDEX IX_Postulaciones_ServicioID ON Postulaciones(ServicioID);
CREATE INDEX IX_Postulaciones_PrestadorID ON Postulaciones(PrestadorID);
CREATE INDEX IX_Mensajes_ConversacionID ON Mensajes(ConversacionID);
CREATE INDEX IX_Mensajes_ReceptorID ON Mensajes(ReceptorID);
CREATE INDEX IX_Notificaciones_UsuarioID ON Notificaciones(UsuarioID);
CREATE INDEX IX_Notificaciones_Leida ON Notificaciones(Leida);
GO

-- ============================================
-- DATOS DE PRUEBA (OPCIONAL)
-- ============================================

-- Usuario de prueba (password: 123456)
INSERT INTO Usuarios (Nombre, NombreCompleto, Email, Password, RolID, Telefono, Ubicacion)
VALUES 
('Admin', 'Administrador HOP', 'admin@hop.com', '123456', 1, '6871234567', 'Los Mochis'),
('Juan Perez', 'Juan Carlos Pérez', 'juan@email.com', '123456', 2, '6871234567', 'Los Mochis'),
('Maria Lopez', 'María Guadalupe López', 'maria@email.com', '123456', 3, '6871234568', 'Los Mochis');
GO

-- Servicios de prueba
INSERT INTO Servicios (Titulo, UsuarioID, Ubicacion, CategoriaID, Descripcion)
VALUES 
('¡Ocupado Muy Pronto! Tu Local, Tu Equipo!', 2, 'Los Mochis', 1, 'Buscamos personal para construcción'),
('Plomero Urgente', 2, 'Los Mochis', 2, 'Se necesita plomero para reparación urgente'),
('Electricista', 3, 'Los Mochis', 3, 'Instalación eléctrica para casa habitación');
GO

PRINT 'Base de datos HOPBD creada exitosamente!';
PRINT 'Credenciales de prueba:';
PRINT 'Admin: admin@hop.com / 123456';
PRINT 'Usuario: juan@email.com / 123456';
PRINT 'Usuario: maria@email.com / 123456';
GO

USE hopbd;
GO

-- ============================================
-- 1. PROCEDIMIENTOS DE USUARIOS
-- ============================================

-- Registrar Usuario
CREATE OR ALTER PROCEDURE RegistrarUsuario
    @Nombre NVARCHAR(100),
    @RolID INT,
    @Email NVARCHAR(100),
    @Password NVARCHAR(255)
AS
BEGIN
    INSERT INTO Usuarios (Nombre, Email, Password, RolID, NombreCompleto, FechaRegistro)
    VALUES (@Nombre, @Email, @Password, @RolID, @Nombre, GETDATE());
    
    SELECT SCOPE_IDENTITY() AS Id;
END
GO

-- Login
CREATE OR ALTER PROCEDURE Login
    @Email NVARCHAR(100),
    @Password NVARCHAR(255)
AS
BEGIN
    SELECT 
        u.Id,
        u.Nombre AS name,
        r.Nombre AS Rol,
        u.NombreCompleto
    FROM Usuarios u
    INNER JOIN Roles r ON u.RolID = r.Id
    WHERE u.Email = @Email AND u.Password = @Password AND u.Activo = 1
END
GO

-- Actualizar Perfil
CREATE OR ALTER PROCEDURE ActualizarPerfil
    @UsuarioID INT,
    @NombreCompleto NVARCHAR(200),
    @Bio NVARCHAR(500),
    @Foto NVARCHAR(500),
    @Tel NVARCHAR(20)
AS
BEGIN
    UPDATE Usuarios 
    SET NombreCompleto = @NombreCompleto,
        Bio = @Bio,
        FotoURL = @Foto,
        Telefono = @Tel
    WHERE Id = @UsuarioID
END
GO

-- ============================================
-- 2. PROCEDIMIENTOS DE SERVICIOS
-- ============================================

-- Crear Servicio
CREATE OR ALTER PROCEDURE CrearServicio
    @Titulo NVARCHAR(200),
    @UsuarioID INT,
    @Ubicacion NVARCHAR(100),
    @CategoriaID INT,
    @Descripcion NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO Servicios (Titulo, UsuarioID, Ubicacion, CategoriaID, Descripcion, FechaRegistro)
    VALUES (@Titulo, @UsuarioID, @Ubicacion, @CategoriaID, @Descripcion, GETDATE())
    
    SELECT SCOPE_IDENTITY() AS Id
END
GO

-- Obtener Servicios (con filtros)
CREATE OR ALTER PROCEDURE ObtenerServicios
    @Busqueda NVARCHAR(200) = NULL,
    @CategoriaID INT = NULL
AS
BEGIN
    SELECT 
        s.Id,
        s.Titulo,
        s.Ubicacion,
        c.Nombre AS Categoria,
        u.Nombre AS Autor,
        s.FechaRegistro
    FROM Servicios s
    INNER JOIN Categorias c ON s.CategoriaID = c.Id
    INNER JOIN Usuarios u ON s.UsuarioID = u.Id
    WHERE s.Estado = 'activo'
        AND (@Busqueda IS NULL OR s.Titulo LIKE '%' + @Busqueda + '%' OR s.Descripcion LIKE '%' + @Busqueda + '%')
        AND (@CategoriaID IS NULL OR s.CategoriaID = @CategoriaID)
    ORDER BY s.FechaRegistro DESC
END
GO

-- ============================================
-- 3. PROCEDIMIENTOS DE POSTULACIONES
-- ============================================

-- Crear Postulación
CREATE OR ALTER PROCEDURE CrearPostulacion
    @ServicioID INT,
    @PrestadorID INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Postulaciones WHERE ServicioID = @ServicioID AND PrestadorID = @PrestadorID)
    BEGIN
        RAISERROR('Ya te has postulado a este servicio', 16, 1)
        RETURN
    END
    
    INSERT INTO Postulaciones (ServicioID, PrestadorID, FechaPostulacion, Estado)
    VALUES (@ServicioID, @PrestadorID, GETDATE(), 'pendiente')
    
    -- Crear notificación
    DECLARE @UsuarioID INT
    SELECT @UsuarioID = UsuarioID FROM Servicios WHERE Id = @ServicioID
    
    INSERT INTO Notificaciones (UsuarioID, Tipo, Contenido, ReferenciaID, FechaCreacion)
    VALUES (@UsuarioID, 'postulacion', 'Un usuario se ha postulado a tu servicio', @ServicioID, GETDATE())
END
GO

-- Obtener Postulaciones por Servicio
CREATE OR ALTER PROCEDURE ObtenerPostulacionesPorServicio
    @ServicioID INT
AS
BEGIN
    SELECT 
        p.Id AS PostulacionID,
        p.FechaPostulacion AS fecha,
        p.Estado,
        u.Nombre AS UsuarioNombre,
        u.NombreCompleto,
        u.Telefono,
        u.FotoURL
    FROM Postulaciones p
    INNER JOIN Usuarios u ON p.PrestadorID = u.Id
    WHERE p.ServicioID = @ServicioID
    ORDER BY p.FechaPostulacion DESC
END
GO

-- ============================================
-- 4. PROCEDIMIENTOS DE CHAT
-- ============================================

-- Obtener o Crear Conversación
CREATE OR ALTER PROCEDURE ObtenerConversacion
    @User1 INT,
    @User2 INT
AS
BEGIN
    DECLARE @ConvId INT
    
    SELECT @ConvId = Id 
    FROM Conversaciones 
    WHERE (Usuario1ID = @User1 AND Usuario2ID = @User2)
       OR (Usuario1ID = @User2 AND Usuario2ID = @User1)
    
    IF @ConvId IS NULL
    BEGIN
        INSERT INTO Conversaciones (Usuario1ID, Usuario2ID, FechaInicio)
        VALUES (@User1, @User2, GETDATE())
        
        SET @ConvId = SCOPE_IDENTITY()
    END
    
    SELECT @ConvId AS id
END
GO

-- Enviar Mensaje
CREATE OR ALTER PROCEDURE EnviarMensaje
    @ConversacionID INT,
    @EmisorID INT,
    @ReceptorID INT,
    @Mensaje NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO Mensajes (ConversacionID, EmisorID, ReceptorID, Mensaje, FechaEnvio)
    VALUES (@ConversacionID, @EmisorID, @ReceptorID, @Mensaje, GETDATE())
    
    UPDATE Conversaciones 
    SET UltimoMensaje = GETDATE() 
    WHERE Id = @ConversacionID
    
    INSERT INTO Notificaciones (UsuarioID, Tipo, Contenido, ReferenciaID, FechaCreacion)
    VALUES (@ReceptorID, 'mensaje', 'Tienes un nuevo mensaje', @ConversacionID, GETDATE())
END
GO

-- Obtener Notificaciones
CREATE OR ALTER PROCEDURE ObtenerNotificaciones
    @UsuarioID INT
AS
BEGIN
    SELECT 
        Id,
        Contenido,
        Tipo,
        Leida,
        FechaCreacion
    FROM Notificaciones
    WHERE UsuarioID = @UsuarioID
    ORDER BY FechaCreacion DESC
END
GO

-- ============================================
-- 5. VERIFICAR TABLAS EXISTENTES
-- ============================================

-- Verificar si la tabla Roles existe y tiene datos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nombre NVARCHAR(50) NOT NULL,
        Descripcion NVARCHAR(200)
    );
    
    INSERT INTO Roles (Nombre, Descripcion) VALUES
    ('admin', 'Administrador del sistema'),
    ('usuario', 'Usuario normal');
END

-- Verificar si la tabla Categorias existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categorias')
BEGIN
    CREATE TABLE Categorias (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500)
    );
    
    INSERT INTO Categorias (Nombre, Descripcion) VALUES
    ('Construcción', 'Servicios de construcción y remodelación'),
    ('Plomería', 'Reparaciones de fontanería'),
    ('Electricidad', 'Instalaciones eléctricas'),
    ('Jardinería', 'Mantenimiento de jardines'),
    ('Limpieza', 'Servicios de limpieza');
END

PRINT 'Todos los procedimientos almacenados han sido creados exitosamente';
GO
-- Verificar si la columna Activo existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Activo' AND object_id = OBJECT_ID('Usuarios'))
BEGIN
    ALTER TABLE Usuarios ADD Activo BIT DEFAULT 1;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'FechaCreacion' AND object_id = OBJECT_ID('Notificaciones'))
BEGIN
    ALTER TABLE Notificaciones ADD FechaCreacion DATETIME DEFAULT GETDATE();
END
GO

USE hopbd;
GO

-- Verificar si el procedimiento existe
IF OBJECT_ID('RegistrarUsuario', 'P') IS NOT NULL
    PRINT 'El procedimiento RegistrarUsuario existe';
ELSE
    PRINT 'El procedimiento RegistrarUsuario NO existe';


CREATE OR ALTER PROCEDURE ActualizarDatosUsuario
    @UsuarioID INT,
    @Telefono NVARCHAR(20),
    @Ubicacion NVARCHAR(100)
AS
BEGIN
    UPDATE Usuarios 
    SET Telefono = @Telefono,
        Ubicacion = @Ubicacion
    WHERE Id = @UsuarioID
END
GO    
    

 
-- Agregar columna de imagen a la tabla Servicios
ALTER TABLE Servicios ADD ImagenURL NVARCHAR(500) NULL;
GO

-- Actualizar la tabla para que tenga una imagen por defecto
UPDATE Servicios SET ImagenURL = '/static/images/vacante.jpg' WHERE ImagenURL IS NULL;
GO

-- Verificar la estructura actualizada
SELECT Id, Titulo, ImagenURL FROM Servicios;
GO

-- Modificar el procedimiento para aceptar la imagen
CREATE OR ALTER PROCEDURE CrearServicio
    @Titulo NVARCHAR(200),
    @UsuarioID INT,
    @Ubicacion NVARCHAR(100),
    @CategoriaID INT,
    @Descripcion NVARCHAR(MAX),
    @ImagenURL NVARCHAR(500) = NULL
AS
BEGIN
    INSERT INTO Servicios (Titulo, UsuarioID, Ubicacion, CategoriaID, Descripcion, ImagenURL, FechaRegistro)
    VALUES (@Titulo, @UsuarioID, @Ubicacion, @CategoriaID, @Descripcion, 
            ISNULL(@ImagenURL, '/static/images/vacante.jpg'), GETDATE())
    
    SELECT SCOPE_IDENTITY() AS Id
END
GO

-- Modificar el procedimiento para incluir la imagen
CREATE OR ALTER PROCEDURE ObtenerServicios
    @Busqueda NVARCHAR(200) = NULL,
    @CategoriaID INT = NULL,
    @UsuarioID INT = NULL
AS
BEGIN
    SELECT 
        s.Id,
        s.Titulo,
        s.Ubicacion,
        c.Nombre AS Categoria,
        u.Nombre AS Autor,
        s.FechaRegistro,
        s.Descripcion,
        s.ImagenURL
    FROM Servicios s
    INNER JOIN Categorias c ON s.CategoriaID = c.Id
    INNER JOIN Usuarios u ON s.UsuarioID = u.Id
    WHERE s.Estado = 'activo'
        AND (@Busqueda IS NULL OR s.Titulo LIKE '%' + @Busqueda + '%' OR s.Descripcion LIKE '%' + @Busqueda + '%')
        AND (@CategoriaID IS NULL OR s.CategoriaID = @CategoriaID)
        AND (@UsuarioID IS NULL OR s.UsuarioID = @UsuarioID)
    ORDER BY s.FechaRegistro DESC
END
GO