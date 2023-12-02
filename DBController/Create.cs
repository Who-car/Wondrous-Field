using Npgsql;

namespace DatabaseController
{
    public partial class DBController
    {
        public async Task<bool> AddUser(User userInfo)
        {
            try
            {
                var commandText = $"INSERT INTO \"user\" (login, \"password\") VALUES(@login, @password)";

                await using (var cmd = new NpgsqlCommand(commandText, _connection))
                {
                    cmd.Parameters.AddWithValue("login", userInfo.Login);
                    cmd.Parameters.AddWithValue("password", await PasswordHasher.HashPasswordAsync(userInfo.Password));

                    await _connection.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
    }
}
