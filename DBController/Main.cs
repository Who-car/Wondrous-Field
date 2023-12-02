using Npgsql;

namespace DatabaseController
{
    public partial class DBController
    {
        string? _connectionString;
        NpgsqlConnection _connection;

        public DBController(string? connectionString)
        {
            _connectionString = connectionString;
            _connection = new NpgsqlConnection(_connectionString);
        }
    }
}
