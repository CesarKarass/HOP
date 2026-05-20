-- Verificar tablas creadas
SELECT name FROM sys.tables;

-- Verificar usuarios insertados
SELECT * FROM Usuarios;

-- Verificar stored procedures
SELECT name FROM sys.procedures;

SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Usuarios'
ORDER BY ORDINAL_POSITION;


