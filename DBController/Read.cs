using Npgsql;

namespace DatabaseController
{
    public partial class DBController
    {
        public async Task<bool> CheckUser(User userLogin)
        {
            try
            {
                var commandText = $"SELECT \"password\" FROM \"user\" where login = @login";

                string? password = null;
                bool arePasswordsEqual = false;
                await using (var cmd = new NpgsqlCommand(commandText, _connection))
                {
                    cmd.Parameters.AddWithValue("login", userLogin.Login);
                    
                    await _connection.OpenAsync();
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            password = reader["password"] as string;
                        }
                        arePasswordsEqual = await PasswordHasher.ArePasswordsEqualAsync(userLogin.Password, password!);
                    }
                }


                return arePasswordsEqual;
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
