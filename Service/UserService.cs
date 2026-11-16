using I.Service;
using Microsoft.Data.SqlClient;
using R.Model;
using SM.Model;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace U.Service
{
    public class UserService : IUserService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        public UserService(IConfiguration configuration, ILogger<UserService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }
        public async Task<List<User>> GetAllUsers()
        {
            _logger.LogInformation("GetAllUsers started.");
            try
            {
                var users = new List<User>();
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_GetAllUsersinfo", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Email = reader.GetString("Email"),
                        PhoneNumber = reader.GetString("PhoneNumber"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy")
                    });
                }
                _logger.LogInformation("GetAllUsers completed successfully. Count: {Count}", users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting all users.");
                throw;
            }
        }

        public async Task<User> GetUserById(int id)
        {
            _logger.LogInformation("GetUserById started. UserId: {UserId}", id);
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_GetUserById", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new User
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Email = reader.GetString("Email"),
                        PhoneNumber = reader.GetString("PhoneNumber"),
                        IsActive = reader.GetBoolean("IsActive"),
                        CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy")
                    };
                }
                _logger.LogWarning("User not found. UserId: {UserId}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user. UserId: {UserId}", id);
                throw;
            }
        }

        public async Task<int> CreateUser(User user)
        {
            _logger.LogInformation("CreateUser started.");
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_CreateUser", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Name", SqlDbType.VarChar, 20).Value = user.Name;
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 30).Value = user.Email;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 50).Value = user.PasswordHash;
                cmd.Parameters.Add("@PhoneNumber", SqlDbType.VarChar, 10).Value = (object?)user.PhoneNumber ?? DBNull.Value;
                cmd.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = (object?)user.CreatedBy ?? DBNull.Value;
                await connection.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                _logger.LogInformation("User created successfully.");
                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                throw;
            }
        }
        public async Task<bool> UpdateUser(int id, User user)
        {
            _logger.LogInformation("UpdateUser started. UserId: {UserId}", id);
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_UpdateUser", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", (object?)user.PhoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UpdatedBy", (object?)user.UpdatedBy ?? DBNull.Value);
                await connection.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                int rows = Convert.ToInt32(result);
                Console.WriteLine("Rows affected: " + rows);
                if (rows > 0)
                {
                    _logger.LogInformation("User updated successfully. UserId: {UserId}", id);
                    return true;
                }
                _logger.LogWarning("User update failed. UserId: {UserId}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user. UserId: {UserId}", id);
                throw;
            }
        }
        public async Task<bool> AssignRoles(int userId, List<int> roleId)
        {
            _logger.LogInformation("AssignRoles started. UserId: {UserId}", userId);
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_AssignUserRoles", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@RoleIds", string.Join(",", roleId));
                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles to user. UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<Role>> GetUserRoles(int userId)
        {
            _logger.LogInformation("GetUserRoles started. UserId: {UserId}", userId);
            try
            {
                var roles = new List<Role>();
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_GetUserRoles", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);
                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    roles.Add(new Role
                    {
                        Id = reader.GetInt32("Id"),
                        RoleName = reader.GetString("RoleName")
                    });
                }
                _logger.LogInformation("GetUserRoles completed. UserId: {UserId}", userId);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles. UserId: {UserId}", userId);
                throw;
            }


        }

        public async Task<bool> DeleteUser(int id)
        {
            _logger.LogInformation("DeleteUser started. UserId: {UserId}", id);
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_DeleteUser", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                await connection.OpenAsync();
                int rows = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                if (rows > 0)
                {
                    _logger.LogInformation("User soft deleted successfully. UserId: {UserId}", id);
                    return true;
                }
                _logger.LogWarning("User not found for deletion. UserId: {UserId}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user. UserId: {UserId}", id);
                throw;
            }
        }  
    }
}
