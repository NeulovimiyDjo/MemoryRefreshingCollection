insert into [SomeServerName].[SomeDatabaseName].[dbo].[SomeTableName]
select * from [SomeServerName2].[SomeDatabaseName].dbo.[SomeTableName]

------------------------------------------------------------------

EXEC sp_addlinkedserver @server = 'SomeServerName'
EXEC sp_addlinkedsrvlogin 'SomeServerName'
                         ,'false'
                         ,NULL
                         ,'login'
                         ,'password'

------------------------------------------------------------------

EXEC sp_dropserver 'SomeServerName', 'droplogins'

------------------------------------------------------------------

select name from sys.servers
SELECT @@servername
