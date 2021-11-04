DECLARE @TableName varchar(255)
DECLARE @IndexName varchar(255)
DECLARE @SQL varchar(max)

DECLARE TableCursor CURSOR FOR
SELECT 
    [i].[name] as IndexName, 
    [o].[name] as TableName
FROM sys.indexes [i] 
INNER JOIN sys.objects [o] ON [i].[object_id] = [o].[object_id]
WHERE [i].[type] = 2 AND [i].[is_unique] = 0 AND [i].[is_primary_key] = 0 AND [o].[type] = 'U'

OPEN TableCursor
FETCH NEXT FROM TableCursor INTO @IndexName, @TableName

WHILE @@FETCH_STATUS = 0
BEGIN
	SET @SQL = 'ALTER INDEX [' + @IndexName + '] ON [' + @TableName + '] REBUILD'
	EXEC (@SQL)
	FETCH NEXT FROM TableCursor INTO @IndexName, @TableName
END

CLOSE TableCursor
DEALLOCATE TableCursor
