USE hopbd;
GO

-- Eliminar el procedimiento anterior
DROP PROCEDURE IF EXISTS ObtenerNotificaciones;
GO

-- Crear el procedimiento actualizado que incluye TODAS las notificaciones
CREATE PROCEDURE ObtenerNotificaciones
    @UsuarioID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- ============================================
    -- MENSAJES RECIBIDOS (de la tabla Mensajes)
    -- ============================================
    SELECT 
        m.Id,
        'Mensaje de ' + u.Nombre AS Contenido,
        'mensaje' AS Tipo,
        ISNULL(m.Leido, 0) AS Leida,
        m.FechaEnvio AS FechaCreacion,
        m.ConversacionID AS ReferenciaId,
        m.EmisorID AS EmisorId,
        u.Nombre AS EmisorNombre,
        m.Mensaje AS MensajeContenido,
        NULL AS Puntuacion,
        NULL AS ComentarioCalificacion,
        NULL AS CalificadorNombre,
        NULL AS ServicioTitulo
    FROM Mensajes m
    INNER JOIN Usuarios u ON m.EmisorID = u.Id
    WHERE m.ReceptorID = @UsuarioID
    
    UNION ALL
    
    -- ============================================
    -- POSTULACIONES RECIBIDAS (cuando alguien se postula a MIS servicios)
    -- ============================================
    SELECT 
        p.Id,
        'Nueva postulación de ' + u.Nombre + ' para tu servicio "' + s.Titulo + '"' AS Contenido,
        'postulacion' AS Tipo,
        0 AS Leida,
        p.FechaPostulacion AS FechaCreacion,
        p.Id AS ReferenciaId,
        p.PrestadorID AS EmisorId,
        u.Nombre AS EmisorNombre,
        p.Mensaje AS MensajeContenido,
        NULL AS Puntuacion,
        NULL AS ComentarioCalificacion,
        NULL AS CalificadorNombre,
        s.Titulo AS ServicioTitulo
    FROM Postulaciones p
    INNER JOIN Usuarios u ON p.PrestadorID = u.Id
    INNER JOIN Servicios s ON p.ServicioID = s.Id
    WHERE s.UsuarioID = @UsuarioID
    
    UNION ALL
    
    -- ============================================
    -- CALIFICACIONES RECIBIDAS
    -- ============================================
    SELECT 
        c.Id,
        'Calificación de ' + u.Nombre AS Contenido,
        'calificacion' AS Tipo,
        ISNULL(c.Leida, 0) AS Leida,
        c.Fecha AS FechaCreacion,
        c.ServicioID AS ReferenciaId,
        c.CalificadorID AS EmisorId,
        u.Nombre AS EmisorNombre,
        NULL AS MensajeContenido,
        c.Puntuacion,
        c.Comentario AS ComentarioCalificacion,
        u.Nombre AS CalificadorNombre,
        s.Titulo AS ServicioTitulo
    FROM Calificaciones c
    INNER JOIN Usuarios u ON c.CalificadorID = u.Id
    INNER JOIN Servicios s ON c.ServicioID = s.Id
    WHERE c.CalificadoID = @UsuarioID
    
    ORDER BY FechaCreacion DESC
END
GO

PRINT 'Procedimiento ObtenerNotificaciones actualizado correctamente';
GO

--------------------------------------------

CREATE OR ALTER PROCEDURE ObtenerMensajesEnviados
    @UsuarioID INT
AS
BEGIN
    SELECT 
        m.Id,
        m.ReceptorID,
        u.Nombre AS ReceptorNombre,
        m.Mensaje,
        m.FechaEnvio
    FROM Mensajes m
    INNER JOIN Usuarios u ON m.ReceptorID = u.Id
    WHERE m.EmisorID = @UsuarioID
    ORDER BY m.FechaEnvio DESC
END
GO

PRINT 'Procedimiento ObtenerMensajesEnviados creado';
GO
------------------------------------------------------------

CREATE OR ALTER PROCEDURE ObtenerPostulacionesRecibidas
    @UsuarioID INT
AS
BEGIN
    SELECT 
        p.Id,
        p.ServicioID,
        s.Titulo AS ServicioTitulo,
        p.PrestadorID,
        u.Nombre AS PrestadorNombre,
        p.Mensaje,
        p.Estado,
        p.FechaPostulacion,
        0 AS Leida
    FROM Postulaciones p
    INNER JOIN Servicios s ON p.ServicioID = s.Id
    INNER JOIN Usuarios u ON p.PrestadorID = u.Id
    WHERE s.UsuarioID = @UsuarioID
    ORDER BY p.FechaPostulacion DESC
END
GO

PRINT 'Procedimiento ObtenerPostulacionesRecibidas creado';
GO
------------------------------------------

-- Obtener postulación por ID
CREATE OR ALTER PROCEDURE ObtenerPostulacionPorId
    @PostulacionID INT
AS
BEGIN
    SELECT 
        p.Id,
        p.ServicioID,
        s.Titulo AS ServicioTitulo,
        p.PrestadorID,
        u.Nombre AS PrestadorNombre,
        p.Mensaje,
        p.Estado,
        p.FechaPostulacion,
        ISNULL(p.Leida, 0) AS Leida,
        '' AS CvRuta
    FROM Postulaciones p
    INNER JOIN Servicios s ON p.ServicioID = s.Id
    INNER JOIN Usuarios u ON p.PrestadorID = u.Id
    WHERE p.Id = @PostulacionID
END
GO

-- Actualizar estado de postulación
CREATE OR ALTER PROCEDURE ActualizarEstadoPostulacion
    @PostulacionID INT,
    @Estado NVARCHAR(20)
AS
BEGIN
    UPDATE Postulaciones 
    SET Estado = @Estado
    WHERE Id = @PostulacionID
END
GO

-- Obtener postulaciones recibidas (versión corregida)
CREATE OR ALTER PROCEDURE ObtenerPostulacionesRecibidas
    @UsuarioID INT
AS
BEGIN
    SELECT 
        p.Id,
        p.ServicioID,
        s.Titulo AS ServicioTitulo,
        p.PrestadorID,
        u.Nombre AS PrestadorNombre,
        p.Mensaje,
        p.Estado,
        p.FechaPostulacion,
        ISNULL(p.Leida, 0) AS Leida,
        '' AS CvRuta
    FROM Postulaciones p
    INNER JOIN Servicios s ON p.ServicioID = s.Id
    INNER JOIN Usuarios u ON p.PrestadorID = u.Id
    WHERE s.UsuarioID = @UsuarioID
    ORDER BY p.FechaPostulacion DESC
END
GO

-- Agregar columna Leida si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Leida' AND object_id = OBJECT_ID('Postulaciones'))
BEGIN
    ALTER TABLE Postulaciones ADD Leida BIT DEFAULT 0;
END
GO

PRINT 'Procedimientos creados correctamente';
GO
------------------------------------------------------

-- Agregar columna CvRuta si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'CvRuta' AND object_id = OBJECT_ID('Postulaciones'))
BEGIN
    ALTER TABLE Postulaciones ADD CvRuta NVARCHAR(500) NULL;
END
GO

-- Verificar estructura de la tabla Postulaciones
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Postulaciones'
ORDER BY ORDINAL_POSITION;
GO

---------------------------------------------------------------

----------------------------------------------------------


-- Actualizar procedimiento para incluir CV
CREATE OR ALTER PROCEDURE CrearPostulacion
    @ServicioID INT,
    @PrestadorID INT,
    @Mensaje NVARCHAR(500) = NULL,
    @CvRuta NVARCHAR(500) = NULL
AS
BEGIN
    -- Verificar si ya existe postulación
    IF EXISTS (SELECT 1 FROM Postulaciones WHERE ServicioID = @ServicioID AND PrestadorID = @PrestadorID)
    BEGIN
        RAISERROR('Ya te has postulado a este servicio', 16, 1)
        RETURN
    END
    
    INSERT INTO Postulaciones (ServicioID, PrestadorID, Mensaje, CvRuta, FechaPostulacion, Estado)
    VALUES (@ServicioID, @PrestadorID, @Mensaje, @CvRuta, GETDATE(), 'pendiente')
    
    -- Crear notificación para el dueño del servicio
    DECLARE @UsuarioID INT
    SELECT @UsuarioID = UsuarioID FROM Servicios WHERE Id = @ServicioID
    
    INSERT INTO Notificaciones (UsuarioID, Tipo, Contenido, ReferenciaID, FechaCreacion, Leida)
    VALUES (@UsuarioID, 'postulacion', 'Un usuario se ha postulado a tu servicio', SCOPE_IDENTITY(), GETDATE(), 0)
END
GO
----------------------------------------


------------------------------------------------------------

-- Agregar columna CvRuta a Usuarios si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'CvRuta' AND object_id = OBJECT_ID('Usuarios'))
BEGIN
    ALTER TABLE Usuarios ADD CvRuta NVARCHAR(500) NULL;
END
GO
-----------------------------------------------------------------
CREATE OR ALTER PROCEDURE ActualizarCvUsuario
    @UsuarioID INT,
    @CvRuta NVARCHAR(500)
AS
BEGIN
    UPDATE Usuarios SET CvRuta = @CvRuta WHERE Id = @UsuarioID
END
GO
--------------------------------------------------------------

-- Tabla para CVs de usuarios
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UsuarioCV')
BEGIN
    CREATE TABLE UsuarioCV (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UsuarioID INT FOREIGN KEY REFERENCES Usuarios(Id),
        Titulo NVARCHAR(200) NOT NULL,
        NombreArchivo NVARCHAR(200) NOT NULL,
        RutaArchivo NVARCHAR(500) NOT NULL,
        FechaSubida DATETIME DEFAULT GETDATE(),
        Activo BIT DEFAULT 1
    );
END
GO

-- Obtener CVs del usuario
CREATE OR ALTER PROCEDURE ObtenerCVsUsuario
    @UsuarioID INT
AS
BEGIN
    SELECT Id, Titulo, NombreArchivo, FechaSubida
    FROM UsuarioCV
    WHERE UsuarioID = @UsuarioID AND Activo = 1
    ORDER BY FechaSubida DESC
END
GO

-- Insertar CV
CREATE OR ALTER PROCEDURE InsertarCV
    @UsuarioID INT,
    @Titulo NVARCHAR(200),
    @NombreArchivo NVARCHAR(200),
    @RutaArchivo NVARCHAR(500)
AS
BEGIN
    INSERT INTO UsuarioCV (UsuarioID, Titulo, NombreArchivo, RutaArchivo, FechaSubida)
    VALUES (@UsuarioID, @Titulo, @NombreArchivo, @RutaArchivo, GETDATE())
    
    SELECT SCOPE_IDENTITY() AS Id
END
GO

-- Agregar columna CvRuta a Postulaciones si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'CvRuta' AND object_id = OBJECT_ID('Postulaciones'))
BEGIN
    ALTER TABLE Postulaciones ADD CvRuta NVARCHAR(500) NULL
END
GO

-- Agregar columna Mensaje a Postulaciones si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Mensaje' AND object_id = OBJECT_ID('Postulaciones'))
BEGIN
    ALTER TABLE Postulaciones ADD Mensaje NVARCHAR(500) NULL
END
GO

-- Modificar procedimiento CrearPostulacion para incluir CV y mensaje
CREATE OR ALTER PROCEDURE CrearPostulacion
    @ServicioID INT,
    @PrestadorID INT,
    @Mensaje NVARCHAR(500) = NULL,
    @CvId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM Postulaciones WHERE ServicioID = @ServicioID AND PrestadorID = @PrestadorID)
    BEGIN
        RAISERROR('Ya te has postulado a este servicio', 16, 1)
        RETURN
    END
    
    DECLARE @CvRuta NVARCHAR(500) = NULL;
    
    IF @CvId IS NOT NULL
    BEGIN
        SELECT @CvRuta = RutaArchivo FROM UsuarioCV WHERE Id = @CvId AND UsuarioID = @PrestadorID
    END
    
    INSERT INTO Postulaciones (ServicioID, PrestadorID, Mensaje, CvRuta, Estado, FechaPostulacion)
    VALUES (@ServicioID, @PrestadorID, @Mensaje, @CvRuta, 'pendiente', GETDATE())
    
    DECLARE @UsuarioID INT
    SELECT @UsuarioID = UsuarioID FROM Servicios WHERE Id = @ServicioID
    
    INSERT INTO Notificaciones (UsuarioID, Tipo, Contenido, ReferenciaID, FechaCreacion, Leida)
    VALUES (@UsuarioID, 'postulacion', 'Nueva postulación para tu servicio', SCOPE_IDENTITY(), GETDATE(), 0)
END
GO

-- Obtener postulaciones recibidas
CREATE OR ALTER PROCEDURE ObtenerPostulacionesRecibidas
    @UsuarioID INT
AS
BEGIN
    SELECT 
        p.Id,
        p.ServicioID,
        s.Titulo AS ServicioTitulo,
        p.PrestadorID,
        u.Nombre AS PrestadorNombre,
        p.Mensaje,
        p.CvRuta,
        p.Estado,
        p.FechaPostulacion
    FROM Postulaciones p
    INNER JOIN Servicios s ON p.ServicioID = s.Id
    INNER JOIN Usuarios u ON p.PrestadorID = u.Id
    WHERE s.UsuarioID = @UsuarioID
    ORDER BY p.FechaPostulacion DESC
END
GO

-- Obtener postulación por ID
CREATE OR ALTER PROCEDURE ObtenerPostulacionPorId
    @PostulacionID INT
AS
BEGIN
    SELECT 
        p.Id,
        p.ServicioID,
        s.Titulo AS ServicioTitulo,
        p.PrestadorID,
        u.Nombre AS PrestadorNombre,
        p.Mensaje,
        p.CvRuta,
        p.Estado,
        p.FechaPostulacion
    FROM Postulaciones p
    INNER JOIN Servicios s ON p.ServicioID = s.Id
    INNER JOIN Usuarios u ON p.PrestadorID = u.Id
    WHERE p.Id = @PostulacionID
END
GO

-- Actualizar estado de postulación
CREATE OR ALTER PROCEDURE ActualizarEstadoPostulacion
    @PostulacionID INT,
    @Estado NVARCHAR(20)
AS
BEGIN
    UPDATE Postulaciones 
    SET Estado = @Estado
    WHERE Id = @PostulacionID
    
    -- Crear notificación para el postulante
    DECLARE @PrestadorID INT, @ServicioID INT, @EstadoMsg NVARCHAR(50)
    
    SELECT @PrestadorID = PrestadorID, @ServicioID = ServicioID
    FROM Postulaciones WHERE Id = @PostulacionID
    
    SET @EstadoMsg = CASE WHEN @Estado = 'aceptado' THEN 'ha sido aceptada' ELSE 'ha sido rechazada' END
    
    INSERT INTO Notificaciones (UsuarioID, Tipo, Contenido, ReferenciaID, FechaCreacion, Leida)
    VALUES (@PrestadorID, 'postulacion', 'Tu postulación para el servicio ' + CAST(@ServicioID AS NVARCHAR) + ' ' + @EstadoMsg, @PostulacionID, GETDATE(), 0)
END
GO

-- Modificar ObtenerNotificaciones para incluir postulaciones
CREATE OR ALTER PROCEDURE ObtenerNotificaciones
    @UsuarioID INT
AS
BEGIN
    -- Notificaciones de mensajes
    SELECT 
        m.Id,
        'Mensaje de ' + u.Nombre AS Contenido,
        'mensaje' AS Tipo,
        ISNULL(m.Leido, 0) AS Leida,
        m.FechaEnvio AS FechaCreacion,
        m.ConversacionID AS ReferenciaId,
        m.EmisorID AS EmisorId,
        u.Nombre AS EmisorNombre,
        m.Mensaje AS MensajeContenido,
        NULL AS Puntuacion,
        NULL AS ComentarioCalificacion,
        NULL AS CalificadorNombre,
        NULL AS ServicioTitulo
    FROM Mensajes m
    INNER JOIN Usuarios u ON m.EmisorID = u.Id
    WHERE m.ReceptorID = @UsuarioID
    
    UNION ALL
    
    -- Notificaciones de calificaciones
    SELECT 
        c.Id,
        'Calificación de ' + u.Nombre AS Contenido,
        'calificacion' AS Tipo,
        ISNULL(c.Leida, 0) AS Leida,
        c.Fecha AS FechaCreacion,
        c.ServicioID AS ReferenciaId,
        c.CalificadorID AS EmisorId,
        u.Nombre AS EmisorNombre,
        NULL AS MensajeContenido,
        c.Puntuacion,
        c.Comentario AS ComentarioCalificacion,
        u.Nombre AS CalificadorNombre,
        s.Titulo AS ServicioTitulo
    FROM Calificaciones c
    INNER JOIN Usuarios u ON c.CalificadorID = u.Id
    INNER JOIN Servicios s ON c.ServicioID = s.Id
    WHERE c.CalificadoID = @UsuarioID
    
    UNION ALL
    
    -- Notificaciones de postulaciones
    SELECT 
        p.Id,
        'Postulación de ' + u.Nombre + ' para tu servicio "' + s.Titulo + '"' AS Contenido,
        'postulacion' AS Tipo,
        0 AS Leida,
        p.FechaPostulacion AS FechaCreacion,
        p.Id AS ReferenciaId,
        p.PrestadorID AS EmisorId,
        u.Nombre AS EmisorNombre,
        p.Mensaje AS MensajeContenido,
        NULL AS Puntuacion,
        NULL AS ComentarioCalificacion,
        NULL AS CalificadorNombre,
        s.Titulo AS ServicioTitulo
    FROM Postulaciones p
    INNER JOIN Usuarios u ON p.PrestadorID = u.Id
    INNER JOIN Servicios s ON p.ServicioID = s.Id
    WHERE s.UsuarioID = @UsuarioID
    
    ORDER BY FechaCreacion DESC
END
GO

PRINT 'Todos los procedimientos han sido creados/actualizados correctamente';
GO
-------------------------------------------------------------
ALTER TABLE Servicios ADD Reclutando BIT DEFAULT 1;
GO
-- Agregar columna Reclutando a la tabla Servicios
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Reclutando' AND object_id = OBJECT_ID('Servicios'))
BEGIN
    ALTER TABLE Servicios ADD Reclutando BIT DEFAULT 1;
    PRINT 'Columna Reclutando agregada a la tabla Servicios';
END
GO

-- Actualizar procedimiento CrearServicio
CREATE OR ALTER PROCEDURE CrearServicio
    @Titulo NVARCHAR(200),
    @UsuarioID INT,
    @Ubicacion NVARCHAR(100),
    @CategoriaID INT,
    @Descripcion NVARCHAR(MAX),
    @ImagenURL NVARCHAR(500),
    @Reclutando BIT = 1
AS
BEGIN
    INSERT INTO Servicios (Titulo, UsuarioID, Ubicacion, CategoriaID, Descripcion, ImagenURL, FechaRegistro, Reclutando)
    VALUES (@Titulo, @UsuarioID, @Ubicacion, @CategoriaID, @Descripcion, @ImagenURL, GETDATE(), @Reclutando)
    
    SELECT SCOPE_IDENTITY() AS Id
END
GO

-- Actualizar procedimiento ActualizarServicio
CREATE OR ALTER PROCEDURE ActualizarServicio
    @Id INT,
    @Titulo NVARCHAR(200),
    @Descripcion NVARCHAR(MAX),
    @Ubicacion NVARCHAR(100),
    @CategoriaID INT,
    @ImagenURL NVARCHAR(500) = NULL,
    @Reclutando BIT = 1
AS
BEGIN
    IF @ImagenURL IS NOT NULL
    BEGIN
        UPDATE Servicios 
        SET Titulo = @Titulo,
            Descripcion = @Descripcion,
            Ubicacion = @Ubicacion,
            CategoriaID = @CategoriaID,
            ImagenURL = @ImagenURL,
            Reclutando = @Reclutando
        WHERE Id = @Id
    END
    ELSE
    BEGIN
        UPDATE Servicios 
        SET Titulo = @Titulo,
            Descripcion = @Descripcion,
            Ubicacion = @Ubicacion,
            CategoriaID = @CategoriaID,
            Reclutando = @Reclutando
        WHERE Id = @Id
    END
END
GO

-- Actualizar procedimiento ObtenerServicios
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
        s.FechaRegistro,
        s.Descripcion,
        s.ImagenURL,
        u.Id AS AutorId,
        s.Reclutando
    FROM Servicios s
    INNER JOIN Categorias c ON s.CategoriaID = c.Id
    INNER JOIN Usuarios u ON s.UsuarioID = u.Id
    WHERE s.Estado = 'activo'
        AND (@Busqueda IS NULL OR s.Titulo LIKE '%' + @Busqueda + '%' OR s.Descripcion LIKE '%' + @Busqueda + '%')
        AND (@CategoriaID IS NULL OR s.CategoriaID = @CategoriaID)
    ORDER BY s.FechaRegistro DESC
END
GO

-- Actualizar procedimiento ObtenerServiciosPorUsuario
CREATE OR ALTER PROCEDURE ObtenerServiciosPorUsuario
    @UsuarioID INT
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
        s.UsuarioID,
        s.ImagenURL,
        s.Reclutando
    FROM Servicios s
    INNER JOIN Categorias c ON s.CategoriaID = c.Id
    INNER JOIN Usuarios u ON s.UsuarioID = u.Id
    WHERE s.UsuarioID = @UsuarioID
    ORDER BY s.FechaRegistro DESC
END
GO

PRINT 'Todos los procedimientos han sido actualizados correctamente';
GO 
--------------------------------------------
CREATE OR ALTER PROCEDURE AgregarPasswordGoogle
    @UsuarioID INT,
    @NuevaPassword NVARCHAR(255)
AS
BEGIN
    UPDATE Usuarios SET Password = @NuevaPassword WHERE Id = @UsuarioID
    SELECT 1 AS Exito
END
GO
-- Procedimiento para obtener postulaciones por ID
CREATE OR ALTER PROCEDURE ObtenerPostulacionPorId
    @PostulacionID INT
AS
BEGIN
    SELECT 
        p.Id,
        p.ServicioID,
        s.Titulo AS ServicioTitulo,
        p.PrestadorID,
        u.Nombre AS PrestadorNombre,
        p.Mensaje,
        p.CvRuta,
        p.Estado,
        p.FechaPostulacion,
        p.Leida
    FROM Postulaciones p
    INNER JOIN Servicios s ON p.ServicioID = s.Id
    INNER JOIN Usuarios u ON p.PrestadorID = u.Id
    WHERE p.Id = @PostulacionID
END
GO

-- Procedimiento para obtener postulaciones recibidas
CREATE OR ALTER PROCEDURE ObtenerPostulacionesRecibidas
    @UsuarioID INT
AS
BEGIN
    SELECT 
        p.Id,
        p.ServicioID,
        s.Titulo AS ServicioTitulo,
        p.PrestadorID,
        u.Nombre AS PrestadorNombre,
        p.Mensaje,
        p.CvRuta,
        p.Estado,
        p.FechaPostulacion,
        p.Leida
    FROM Postulaciones p
    INNER JOIN Servicios s ON p.ServicioID = s.Id
    INNER JOIN Usuarios u ON p.PrestadorID = u.Id
    WHERE s.UsuarioID = @UsuarioID
    ORDER BY p.FechaPostulacion DESC
END
GO
USE hopbd;
GO

-- Modificar el procedimiento EliminarServicio para eliminar primero las calificaciones
CREATE OR ALTER PROCEDURE EliminarServicio
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Eliminar las calificaciones asociadas al servicio
    DELETE FROM Calificaciones WHERE ServicioID = @Id;
    
    -- 2. Eliminar las postulaciones asociadas al servicio
    DELETE FROM Postulaciones WHERE ServicioID = @Id;
    
    -- 3. Eliminar las imágenes asociadas al servicio
    DELETE FROM ServicioImagenes WHERE ServicioId = @Id;
    
    -- 4. Finalmente eliminar el servicio
    DELETE FROM Servicios WHERE Id = @Id;
    
    SELECT 1 AS Exito;
END
GO

PRINT 'Procedimiento EliminarServicio actualizado correctamente';
GO
-- Ver la constraint actual
SELECT 
    fk.name AS FK_Name,
    tp.name AS ParentTable,
    ref.name AS ReferencedTable
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables ref ON fk.referenced_object_id = ref.object_id
WHERE tp.name = 'Calificaciones' AND ref.name = 'Servicios';

-- Eliminar la constraint actual (reemplaza 'FK__Calificac__Servi__0D7A0286' con el nombre que veas)
ALTER TABLE Calificaciones DROP CONSTRAINT FK__Calificac__Servi__0D7A0286;
GO

-- Crear la constraint con ON DELETE CASCADE
ALTER TABLE Calificaciones
ADD CONSTRAINT FK_Calificaciones_Servicios 
FOREIGN KEY (ServicioID) REFERENCES Servicios(Id) 
ON DELETE CASCADE;
GO

-- Hacer lo mismo para Postulaciones si es necesario
-- Verificar si existe la constraint
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('Postulaciones') AND name LIKE '%FK%Servicios%')
BEGIN
    DECLARE @ConstraintName NVARCHAR(200);
    SELECT @ConstraintName = name FROM sys.foreign_keys 
    WHERE parent_object_id = OBJECT_ID('Postulaciones') AND referenced_object_id = OBJECT_ID('Servicios');
    
    EXEC('ALTER TABLE Postulaciones DROP CONSTRAINT ' + @ConstraintName);
    
    ALTER TABLE Postulaciones
    ADD CONSTRAINT FK_Postulaciones_Servicios 
    FOREIGN KEY (ServicioID) REFERENCES Servicios(Id) 
    ON DELETE CASCADE;
END
GO

PRINT 'Constraints actualizadas con ON DELETE CASCADE';
GO
CREATE OR ALTER PROCEDURE EliminarServicio
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Eliminar calificaciones asociadas
    DELETE FROM Calificaciones WHERE ServicioID = @Id;
    
    -- Eliminar postulaciones asociadas
    DELETE FROM Postulaciones WHERE ServicioID = @Id;
    
    -- Eliminar imágenes asociadas
    DELETE FROM ServicioImagenes WHERE ServicioId = @Id;
    
    -- Eliminar el servicio
    DELETE FROM Servicios WHERE Id = @Id;
    
    SELECT 1 AS Exito;
END
GO



-- Procedimiento para eliminar una notificación por ID
CREATE OR ALTER PROCEDURE EliminarNotificacion
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Notificaciones WHERE Id = @Id;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Exito;
    ELSE
        SELECT 0 AS Exito, 'Notificación no encontrada' AS Error;
END
GO

PRINT 'Procedimiento EliminarNotificacion creado correctamente';
GO


-- Crear procedimiento para eliminar notificación
CREATE OR ALTER PROCEDURE EliminarNotificacion
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Notificaciones WHERE Id = @Id;
    
    IF @@ROWCOUNT > 0
        SELECT 1 AS Exito;
    ELSE
        SELECT 0 AS Exito, 'Notificación no encontrada' AS Error;
END
GO

PRINT 'Procedimiento EliminarNotificacion creado correctamente';
GO
CREATE OR ALTER PROCEDURE EliminarNotificacion
    @Id INT
AS
BEGIN
    DELETE FROM Notificaciones WHERE Id = @Id;
    SELECT 1 AS Exito;
END
GO
CREATE OR ALTER PROCEDURE EliminarUsuario
    @UsuarioId INT
AS
BEGIN
    -- Primero eliminar registros relacionados con FK
    DELETE FROM Notificaciones WHERE UsuarioID = @UsuarioId;
    DELETE FROM Mensajes WHERE EmisorID = @UsuarioId OR ReceptorID = @UsuarioId;
    DELETE FROM Postulaciones WHERE PrestadorID = @UsuarioId;
    DELETE FROM Calificaciones WHERE CalificadorID = @UsuarioId OR CalificadoID = @UsuarioId;
    DELETE FROM Servicios WHERE UsuarioID = @UsuarioId;
    
    -- Finalmente eliminar el usuario
    DELETE FROM Usuarios WHERE Id = @UsuarioId;
END
GO