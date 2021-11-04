using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Unity;

namespace MyProjectTests
{
    public class DbSetupManager
    {
        private bool DbCreatedFlag = false;
        private string dbCode;

        public DbSetupManager(string dbCode)
        {
            this.dbCode = dbCode;
        }


        private static SemaphoreSlim _createDbSemaphore = new(1, 1);
        private class SemaphoreScope : IDisposable
        {
            private SemaphoreSlim _semaphore;
            private bool _entered = false;
            public SemaphoreScope(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }
            public async Task<SemaphoreScope> Enter()
            {
                await _semaphore.WaitAsync();
                _entered = true;
				return this;
            }
            public void Dispose()
            {
                if (_entered)
                    _semaphore.Release();
                _entered = false;
            }
        }
		
        public async Task CreateDbAndInstall()
        {
            string binDir = Assembly.GetExecutingAssembly().Location.Split("bin")[0] + "bin\\";
            if (File.Exists(binDir + "dbcreatedflag"))
                DbCreatedFlag = true;

            using SemaphoreScope semaphoreScope = new(_createDbSemaphore);
            if (!DbCreatedFlag)
                await semaphoreScope.Enter();

            if (File.Exists(binDir + "dbcreatedflag")) // Check again, other thread may have created it.
                DbCreatedFlag = true;

            if (!DbCreatedFlag)
            {
                await CreateDatabase(dbCode);
                await BackupDatabase(dbCode);
                File.Create(binDir + "dbcreatedflag");
            }
			else
			{
				await RestoreDatabase(dbCode);
			}

            await CreateSnapshot(dbCode);
        }

        public async Task DeleteDb()
        {
			await DeleteSnapshot(dbCode);
			await DropDatabase(dbCode);
        }

        public async Task ResetDb()
        {
            await RestoreSnapshot(dbCode);
        }




        private static Task CreateDatabase(string dbCode)
        {
            string query = @$"
DECLARE @DefaultDataPath NVARCHAR(512);
DECLARE @DefaultLogPath NVARCHAR(512);
SELECT
    @DefaultDataPath = CAST(SERVERPROPERTY('instancedefaultdatapath') AS NVARCHAR(512)),
    @DefaultLogPath = CAST(SERVERPROPERTY('instancedefaultlogpath') AS NVARCHAR(512))

DECLARE @Query NVARCHAR(MAX) =  N'
CREATE DATABASE [{databaseName}]
    ON PRIMARY (NAME = N''{templateDbName}'', FILENAME = N''' + @DefaultDataPath + '{databaseName}.mdf'')
    LOG ON (NAME = N''{templateDbName}_log'', FILENAME = N''' + @DefaultLogPath + '{databaseName}_log.ldf'');
ALTER DATABASE [{databaseName}] SET RECOVERY SIMPLE;
';
EXEC sp_executesql @Query;
";

            await Execute(query);
        }

        private static Task DropDatabase(string dbCode)
        {
            StringBuilder builder = new();

            builder
				.Append("IF EXISTS (SELECT * FROM [sys].[databases] WHERE [name] = '")
				.Append(databaseName)
				.Append("')")
				.AppendLine()
				.Append("BEGIN")
				.AppendLine()
				.Append("\tALTER DATABASE ")
				.Append(databaseName)
				.Append(" SET OFFLINE WITH ROLLBACK IMMEDIATE;")
				.AppendLine()
				.Append("\tALTER DATABASE ")
				.Append(databaseName)
				.Append(" SET ONLINE;")
				.AppendLine()
				.Append("\tDROP DATABASE ")
				.Append(databaseName)
				.Append(';')
				.AppendLine()
				.Append("END;");

            await Execute(builder.ToString());
        }
		

        private static Task RestoreDatabase(string dbCode)
        {
            string query = @$"
DECLARE @DefaultDataPath NVARCHAR(512);
DECLARE @DefaultLogPath NVARCHAR(512);
SELECT
    @DefaultDataPath = CAST(SERVERPROPERTY('instancedefaultdatapath') AS NVARCHAR(512)),
    @DefaultLogPath = CAST(SERVERPROPERTY('instancedefaultlogpath') AS NVARCHAR(512))

DECLARE @Query NVARCHAR(MAX) =  N'
RESTORE DATABASE [{databaseName}] FROM DISK = ''{templateDbName}.bak'' WITH
MOVE N''{templateDbName}'' TO N''' + @DefaultDataPath + '{databaseName}.mdf'',
MOVE N''{templateDbName}_log'' TO N''' + @DefaultLogPath + '{databaseName}_log.ldf''
';
EXEC sp_executesql @Query;
";

            await Execute(query);
        }

        public static async Task BackupDatabase(string dbCode)
        {
            StringBuilder builder = new();
            builder
                .Append("BACKUP DATABASE ")
                .Append(databaseName)
                .Append("TO DISK = '")
                .Append(templateDbName)
                .Append(".bak' ")
                .Append("WITH FORMAT,INIT;");

            await Execute(builder.ToString());
        }
		

        public static async Task CreateSnapshot(string dbCode)
        {
            string query = @$"
DECLARE @SnapshotFilePath NVARCHAR(512) =
(
    SELECT
        REPLACE(mf.physical_name, '.mdf', '_snapshot.ss')
	FROM sys.master_files mf
	INNER JOIN sys.databases db ON db.database_id = mf.database_id
	WHERE db.name = N'{databaseName[0]}'
		AND mf.type = 0
);
DECLARE @Query NVARCHAR(MAX) = '
CREATE DATABASE [{databaseName[0]}_snapshot] ON
(
    NAME = N''{templateDbName}'',
    FILENAME = ''' + @SnapshotFilePath + '''
) AS SNAPSHOT OF [{databaseName[0]}];
'
EXEC sp_executesql @Query
";

            command.CommandText = query;
            await Execute(query);
        }

        public static async Task DeleteSnapshot(string dbCode)
        {
            StringBuilder builder = new();
            builder
                .Append("IF EXISTS (SELECT TOP 1 1 FROM [sys].[databases] WHERE [name] = '")
                .Append(databaseName)
                .Append("' AND source_database_id IS NOT NULL)")
                .AppendLine()
                .Append("BEGIN")
                .AppendLine()
                .Append("\tDROP DATABASE ")
                .Append(databaseName + "_snapshot")
                .Append(";")
                .AppendLine()
                .Append("END;")
                ;

            await Execute(builder.ToString());
        }

        public static async Task RestoreSnapshot(string dbCode)
        {
            StringBuilder builder = new();
            builder
                .Append("\tALTER DATABASE ")
                .Append(databaseName)
                .Append(" SET SINGLE_USER WITH ROLLBACK IMMEDIATE;")
                .AppendLine()
                .Append("\tRESTORE DATABASE ")
                .Append(databaseName)
                .Append(" FROM DATABASE_SNAPSHOT = '")
                .Append(databaseName + "_snapshot")
                .Append("';")
                .AppendLine()
                .Append("\tALTER DATABASE ")
                .Append(databaseName)
                .Append(" SET MULTI_USER WITH ROLLBACK IMMEDIATE;")
                ;

            await Execute(builder.ToString());
        }
    }
}
