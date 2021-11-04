Add-Type -Path ".\Npgsql.dll"

$connectionString = "server=localhost;port=5432;user id=postgres;password=somepassword;database=somedatabase"
$connection = New-Object Npgsql.NpgsqlConnection;
$connection.ConnectionString = $connectionString
$connection.Open()

$query = "select * from pg_catalog.pg_class limit 2;"
$command = $connection.CreateCommand()
$command.CommandText = $query
$adapter = New-Object -TypeName Npgsql.NpgsqlDataAdapter $command
$dataset = New-Object -TypeName System.Data.DataSet
$adapter.Fill($dataset)
$dataset.Tables[0]

$connection.Close()