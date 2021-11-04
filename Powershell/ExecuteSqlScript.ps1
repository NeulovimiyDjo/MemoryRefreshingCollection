$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
Set-StrictMode -Version 3.0

function ExecuteSqlScript([string]$scriptPath, [string]$databaseName) {
    $connectionStringBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $DefaultConnectionString
    $connectionStringBuilder["Initial Catalog"] = $databaseName

    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
    $sqlConnection.ConnectionString = $connectionStringBuilder.ConnectionString

    $sqlConnection.Open()
    try {
        $sqlCommand = $sqlConnection.CreateCommand()
        $sqlCommand.CommandTimeout = 60
        $scriptContent = Get-Content $scriptPath -Raw
        $batchTextsList = $scriptContent -Split "\bgo\b", 0, "RegexMatch,IgnoreCase,Singleline" |
            ForEach-Object { $_.Trim() } |
            Where-Object { $_ }

        foreach ($batchText in $batchTextsList) {
            $sqlCommand.CommandText = $batchText
            $sqlCommand.ExecuteNonQuery()
        }
    } finally {
        $sqlConnection.Close()
    }
}

function DeleteDatabase([string]$databaseName) {
	$connectionString = "Data Source=localhost; Integrated Security=true"    
	$connection = New-Object System.Data.SqlClient.SQLConnection($connectionString)
	$connection.Open()
	try {
		$sql = @"
if exists (select top 1 1 from master.sys.databases where name = N'$databaseName')
begin
alter database [$databaseName] set single_user with rollback immediate;
alter database [$databaseName] set offline with rollback immediate;
alter database [$databaseName] set online;
drop database [$databaseName];
end;
"@
		$command = New-Object System.Data.SqlClient.SqlCommand($sql, $connection)
		$command.CommandTimeout = 180
		$command.ExecuteNonQuery()
	}
	finally {
		$connection.Close()
	}
}
