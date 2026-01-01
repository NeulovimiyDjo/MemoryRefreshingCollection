using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace NpgsqlExecutorProject;

internal static class NpgsqlExecutor
{
    private static readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
        .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
        .AddJsonFile(
                path: "appsettings.json",
                optional: false,
                reloadOnChange: true)
        .Build();
    private static readonly string _connectionString = _configuration
        .GetSection("AppSettings").GetValue<string>("ConnectionString");

    public static async Task<int> Execute(
        string sql, params NpgsqlParameter[] parameters)
    {
        await using NpgsqlConnection conn = await OpenConnection();
        await using NpgsqlCommand cmd = CreateCommand(conn, sql, parameters);
        return await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<IEnumerable<TRes>> Query<TRes>(
        string sql, params NpgsqlParameter[] parameters)
    {
        await using NpgsqlConnection conn = await OpenConnection();
        await using NpgsqlCommand cmd = CreateCommand(conn, sql, parameters);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        List<TRes> res = new();
        while (await reader.ReadAsync())
            res.Add((TRes)reader.GetValue(0));
        return res;
    }

    public static async Task<IEnumerable<(TRes1, TRes2)>> Query<TRes1, TRes2>(
        string sql, params NpgsqlParameter[] parameters)
    {
        await using NpgsqlConnection conn = await OpenConnection();
        await using NpgsqlCommand cmd = CreateCommand(conn, sql, parameters);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        List<(TRes1, TRes2)> res = new();
        while (await reader.ReadAsync())
            res.Add(((TRes1)reader.GetValue(0), (TRes2)reader.GetValue(1)));
        return res;
    }

    public static async Task<IEnumerable<(TRes1, TRes2, TRes3)>> Query<TRes1, TRes2, TRes3>(
        string sql, params NpgsqlParameter[] parameters)
    {
        await using NpgsqlConnection conn = await OpenConnection();
        await using NpgsqlCommand cmd = CreateCommand(conn, sql, parameters);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        List<(TRes1, TRes2, TRes3)> res = new();
        while (await reader.ReadAsync())
            res.Add(((TRes1)reader.GetValue(0), (TRes2)reader.GetValue(1), (TRes3)reader.GetValue(2)));
        return res;
    }

    public static async Task<TRes> QuerySingleOrDefault<TRes>(
        string sql, params NpgsqlParameter[] parameters)
    {
        await using NpgsqlConnection conn = await OpenConnection();
        await using NpgsqlCommand cmd = CreateCommand(conn, sql, parameters);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync()
            ? (TRes)reader.GetValue(0)
            : default;
    }

    public static async Task<(TRes1, TRes2)> QuerySingleOrDefault<TRes1, TRes2>(
        string sql, params NpgsqlParameter[] parameters)
    {
        await using NpgsqlConnection conn = await OpenConnection();
        await using NpgsqlCommand cmd = CreateCommand(conn, sql, parameters);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync()
            ? ((TRes1)reader.GetValue(0), (TRes2)reader.GetValue(1))
            : default;
    }

    public static async Task<(TRes1, TRes2, TRes3)> QuerySingleOrDefault<TRes1, TRes2, TRes3>(
        string sql, params NpgsqlParameter[] parameters)
    {
        await using NpgsqlConnection conn = await OpenConnection();
        await using NpgsqlCommand cmd = CreateCommand(conn, sql, parameters);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync()
            ? ((TRes1)reader.GetValue(0), (TRes2)reader.GetValue(1), (TRes3)reader.GetValue(2))
            : default;
    }

    private static async Task<NpgsqlConnection> OpenConnection()
    {
        NpgsqlConnection conn = new(_connectionString);
        await conn.OpenAsync();
        return conn;
    }

    private static NpgsqlCommand CreateCommand(NpgsqlConnection conn, string sql, NpgsqlParameter[] parameters)
    {
        NpgsqlCommand cmd = new(sql, conn);
        foreach (NpgsqlParameter param in parameters)
            cmd.Parameters.Add(param);
        return cmd;
    }
}
