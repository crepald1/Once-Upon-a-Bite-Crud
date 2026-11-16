using MySqlConnector;
using System.Data;

namespace OnceUponABite.Data;

public static class Db
{
    private const string ConnectionString =
   "Server=localhost;Port=3306;Database=once_upon_a_bite;Uid=root;Pwd=;";

    public static MySqlConnection GetConnection()
        => new MySqlConnection(ConnectionString);

    public static DataTable Query(string sql, params MySqlParameter[] parameters)
    {
        using var connection = GetConnection();
        connection.Open();

        using var command = new MySqlCommand(sql, connection);

        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        using var adapter = new MySqlDataAdapter(command);

        var table = new DataTable();
        adapter.Fill(table);

        return table;
    }

    public static int Execute(string sql, params MySqlParameter[] parameters)
    {
        using var connection = GetConnection();
        connection.Open();

        using var command = new MySqlCommand(sql, connection);

        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        return command.ExecuteNonQuery();
    }

    public static object? Scalar(string sql, params MySqlParameter[] parameters)
    {
        using var connection = GetConnection();
        connection.Open();

        using var command = new MySqlCommand(sql, connection);

        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        var result = command.ExecuteScalar();

        return result == DBNull.Value ? null : result;
    }

    public static MySqlParameter P(string name, object? value)
        => new MySqlParameter(name, value ?? DBNull.Value);
}