create database DatabaseName_snapshot ON (
  NAME = N'LogicalDatabaseFileName',
  FILENAME = N'C:\SomePath\SnapshotFileName.ss'
) as snapshot of DatabaseName;

-----------------------------------------------------------------

ALTER DATABASE DatabaseName SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

restore database DatabaseName from DATABASE_SNAPSHOT = 'DatabaseName_snapshot';

ALTER DATABASE DatabaseName SET MULTI_USER WITH ROLLBACK IMMEDIATE;

-----------------------------------------------------------------

select * from sys.database_files -- to find logical database name