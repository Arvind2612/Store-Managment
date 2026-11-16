//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Store_Managment.jwtservice;
using Store_Managment.Models;
using Store_Managment.Services;
using System.Data;





namespace Store_Managment.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _connectionString;
        private readonly IJwtService _jwtService;
        private readonly Jwtsettings _jwtsettings;

        // Constructor
        public AuthService(IConfiguration configuration, IJwtService jwtService, IOptions<Jwtsettings> jwtsettings)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")!;

            _jwtService = jwtService;
            _jwtsettings = jwtsettings.Value;
        }

        // Login Method

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            using (SqlConnection connection =
                new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command =
                    new SqlCommand("SP_GetUserinLogin", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(
                        "@Email", SqlDbType.VarChar, 30
                    ).Value = request.Email;

                    using (SqlDataReader reader =
                        await command.ExecuteReaderAsync())
                    {
                        // Check whether user exists
                        if (!await reader.ReadAsync())
                        {
                            return null;
                        }

                        // Read user details
                        int userId = reader.GetInt32(
                            reader.GetOrdinal("Id"));

                        string name = reader.GetString(
                            reader.GetOrdinal("Name"));

                        string email = reader.GetString(
                            reader.GetOrdinal("Email"));

                        string passwordHash = reader.GetString(
                            reader.GetOrdinal("PasswordHash"));

                        bool isActive = reader.GetBoolean(
                            reader.GetOrdinal("IsActive"));

                        // Verify password
                        if (!isActive ||
                            !BCrypt.Net.BCrypt.Verify(
                                request.Password, passwordHash))
                        {
                            return null;
                        }

                        // Token expiry (must match JWT configuration)
                        DateTime accessTokenExpiry =
                            DateTime.UtcNow.AddMinutes(_jwtsettings.AccessTokenExpiryMinutes);

                        DateTime refreshTokenExpiry =
                            DateTime.UtcNow.AddDays(_jwtsettings.RefreshTokenExpiryDays);

                        // Generate access token
                        string accessToken =
                            _jwtService.GenerateAccessToken(
                                userId,
                                name,
                                email,
                                new List<string>());

                        // Generate refresh token
                        string refreshToken =
                            _jwtService.GenerateRefreshToken();

                        // Return login response
                        return new LoginResponse
                        {
                            UserId = userId,
                            UserName = name,
                            Email = email,
                            AccessToken = accessToken,
                            RefreshToken = refreshToken,
                            AccessTokenExpiresAt = accessTokenExpiry,
                            RefreshTokenExpiresAt = refreshTokenExpiry
                        };
                    }
                }
            }
        }
        //logout Method
        public async Task<bool> LogoutAsync(Logout request)
        {
            // Step 1: Convert refresh token into hash
            string tokenHash = _jwtService.HashToken(request.RefreshToken);

            // Step 2: Create SQL connection
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            // Step 3: Open connection
            await connection.OpenAsync();

            // Step 4: Create SQL command
            using SqlCommand command =
                new SqlCommand("SP_userLogout", connection);

            // Step 5: Specify stored procedure
            command.CommandType = CommandType.StoredProcedure;

            // Step 6: Pass parameter
            command.Parameters.Add("@TokenHash", SqlDbType.VarChar, 64)
                .Value = tokenHash;

            // Step 7: Execute stored procedure
            object? result = await command.ExecuteScalarAsync();

            // Step 8: Convert result into integer
            int rowsAffected = Convert.ToInt32(result);

            // Step 9: Return result
            return rowsAffected > 0;
        }
        //Refresh Token Method
        public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // Step 1: Hash old refresh token
            string oldTokenHash =
                _jwtService.HashToken(request.RefreshToken);

            // Step 2: Generate new refresh token
            string newRefreshToken =
                _jwtService.GenerateRefreshToken();

            // Step 3: Hash new refresh token
            string newTokenHash =
                _jwtService.HashToken(newRefreshToken);

            // Step 4: Calculate new expiry
            DateTime newExpiry =
                DateTime.UtcNow.AddDays(_jwtsettings.RefreshTokenExpiryDays);

            // Step 5: Create SQL connection
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            // Step 6: Create SQL command
            using SqlCommand command =
                new SqlCommand("SP_NewRefreshToken", connection);

            // Step 7: Specify stored procedure
            command.CommandType = CommandType.StoredProcedure;

            // Step 8: Pass old token hash
            command.Parameters.Add("@OldTokenHash", SqlDbType.VarChar, 64)
                .Value = oldTokenHash;

            // Step 9: Pass new token hash
            command.Parameters.Add("@NewTokenHash", SqlDbType.VarChar, 64)
                .Value = newTokenHash;

            // Step 10: Pass new expiry
            command.Parameters.Add("@NewExpiry", SqlDbType.DateTime2)
                .Value = newExpiry;

            // Step 11: Execute stored procedure
            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            // Step 12: Check whether data is returned
            if (!await reader.ReadAsync())
                return null;

            // Step 13: Read Result
            int result = reader.GetInt32(
                reader.GetOrdinal("Result"));

            if (result != 1)
                return null;

            // Step 14: Read user details
            int userId = reader.GetInt32(
                reader.GetOrdinal("UserId"));

            string name = reader.GetString(
                reader.GetOrdinal("Name"));

            string email = reader.GetString(
                reader.GetOrdinal("Email"));

            // Step 15: Generate new access JWT
            string newAccessToken =
                _jwtService.GenerateAccessToken(
                    userId,
                    name,
                    email,
                    new List<string>());

            // Step 16: Return new tokens
            return new LoginResponse
            {
                UserId = userId,

                AccessToken = newAccessToken,

                RefreshToken = newRefreshToken,

                AccessTokenExpiresAt =
                    DateTime.UtcNow.AddMinutes(
                        _jwtsettings.AccessTokenExpiryMinutes)
            };
        }
        //Forget Pass 
        public async Task<bool> ForgotPasswordAsync(ForgetPass request)
        {
            // Step 1: Generate reset token
            string resetToken =
                _jwtService.GenerateRefreshToken();

            Console.WriteLine("RESET TOKEN: " + resetToken); // For debugging purposes, remove in production

            // Step 2: Hash reset token
            string tokenHash =
                _jwtService.HashToken(resetToken);

            // Step 3: Calculate expiry
            DateTime expiry =
                DateTime.UtcNow.AddMinutes(15);

            // Step 4: Create SQL connection
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            // Step 5: Create SQL command
            using SqlCommand command =
                new SqlCommand("SP_ForgetPassword", connection);

            // Step 6: Specify stored procedure
            command.CommandType = CommandType.StoredProcedure;

            // Step 7: Pass Email
            command.Parameters.Add(
                "@Email", SqlDbType.VarChar, 30)
                .Value = request.Email;

            // Step 8: Pass TokenHash
            command.Parameters.Add(
                "@TokenHash", SqlDbType.VarChar, 64)
                .Value = tokenHash;

            // Step 9: Pass Expiry
            command.Parameters.Add(
                "@Expiry", SqlDbType.DateTime2)
                .Value = expiry;

            // Step 10: Execute stored procedure
            object? result =
                await command.ExecuteScalarAsync();

            // Step 11: Check result
            return result != null &&
                   result != DBNull.Value &&
                   Convert.ToInt32(result) == 1;
        }
        //Reset Password Method 
        public async Task<bool> ResetPasswordAsync(ResetPassword request)
        {
            // Step 1: Validate request
            if (string.IsNullOrWhiteSpace(request.ResetToken) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return false;
            }

            // Step 2: Hash the reset token
            string tokenHash = _jwtService.HashToken(request.ResetToken);

            // Step 3: Hash the new password using BCrypt
            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Step 4: Create SQL connection
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            // Step 5: Open SQL connection
            await connection.OpenAsync();

            // Step 6: Create SQL command
            using SqlCommand command =
                new SqlCommand("SP_ResetuserPassword", connection);

            // Step 7: Specify command type
            command.CommandType = CommandType.StoredProcedure;

            // Step 8: Pass reset token hash
            command.Parameters.Add("@TokenHash", SqlDbType.VarChar, 64)
                .Value = tokenHash;

            // Step 9: Pass new password hash
            command.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255)
                .Value = passwordHash;

            // Step 10: Execute stored procedure
            object? result = await command.ExecuteScalarAsync();

            // Step 11: Read SQL result
            if (result == null || result == DBNull.Value)
            {
                return false;
            }

            int rowsAffected = Convert.ToInt32(result);

            // Step 12: Return success or failure
            return rowsAffected == 1;
        }
        //Change Password Method 
        public async Task<bool> ChangePasswordAsync(Changepassword request)
        {
            // Step 1: Validate request
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return false;
            }

            // Step 2: Create SQL connection
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            // Step 3: Get the current password hash by email
            string? currentPasswordHash;

            using (SqlCommand getCmd = new SqlCommand(
                "SELECT PasswordHash FROM Users WHERE Email = @Email",
                connection))
            {
                getCmd.Parameters.Add("@Email", SqlDbType.VarChar, 100)
                    .Value = request.Email;

                object? result = await getCmd.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    return false;
                }

                currentPasswordHash = result.ToString();
            }

            // Step 4: Verify the current password
            if (!BCrypt.Net.BCrypt.Verify(
                    request.CurrentPassword, currentPasswordHash))
            {
                return false;
            }

            // Step 5: Hash the new password and save it
            string newHash =
                BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            using SqlCommand updateCmd = new SqlCommand(
                "UPDATE Users SET PasswordHash = @Hash WHERE Email = @Email",
                connection);

            updateCmd.Parameters.Add("@Hash", SqlDbType.VarChar, 100)
                .Value = newHash;
            updateCmd.Parameters.Add("@Email", SqlDbType.VarChar, 100)
                .Value = request.Email;

            return await updateCmd.ExecuteNonQueryAsync() > 0;
        }
        //Logged in User Method

        public async Task<UserLoggedin?> GetCurrentUserAsync(int userId)
        {
            // Step 1: Validate UserId
            if (userId <= 0)
            {
                return null;
            }

            // Step 2: Create SQL connection
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            // Step 3: Create SQL command
            using SqlCommand command =
                new SqlCommand("SP_loggedInUser", connection);

            // Step 4: Specify command type
            command.CommandType = CommandType.StoredProcedure;

            // Step 5: Pass UserId
            command.Parameters.Add("@UserId", SqlDbType.Int)
                .Value = userId;

            // Step 6: Execute stored procedure
            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            // Step 7: Check whether user exists
            if (!await reader.ReadAsync())
            {
                return null;
            }

            // Step 8: Read user details
            UserLoggedin user = new UserLoggedin
            {
                UserId = reader.GetInt32(reader.GetOrdinal("Id")),

                UserName = reader.GetString(reader.GetOrdinal("Name")),

                Email = reader.GetString(reader.GetOrdinal("Email")),

                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),

            };

            // Step 9: Return user details
            return user;
        }
        //Get User Permission Method

        public async Task<List<UserPermission>> GetPermissionsAsync(int userId)
        {
            // Step 1: Validate UserId
            if (userId <= 0)
            {
                return new List<UserPermission>();
            }

            // Step 2: Create SQL connection
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            // Step 3: Create SQL command
            using SqlCommand command =
                new SqlCommand("SP_Get_userpermissions", connection);

            // Step 4: Specify command type
            command.CommandType = CommandType.StoredProcedure;

            // Step 5: Pass UserId
            command.Parameters.Add("@UserId", SqlDbType.Int)
                .Value = userId;

            // Step 6: Execute stored procedure
            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            // Step 7: Create permission list
            List<UserPermission> permissions = new List<UserPermission>();

            // Step 8: Read all permission records
            while (await reader.ReadAsync())
            {
                UserPermission permission = new UserPermission
                {
                    PermissionId =
                        reader.GetInt32(reader.GetOrdinal("PermissionId")),

                    PermissionName =
                        reader.GetString(reader.GetOrdinal("PermissionName"))

                };

                permissions.Add(permission);
            }

            // Step 9: Return permission list
            return permissions;
        }
        //Register Method
        public async Task<int> RegisterAsync(Register request)
        {
            // Step 1: Hash the password using BCrypt
            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Step 2: Create SQL connection
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync();

            // Step 3: Call stored procedure
            using SqlCommand command =
                new SqlCommand("SP_RegisterUsers", connection);

            command.CommandType = CommandType.StoredProcedure;

            // Step 4: Pass parameters
            command.Parameters.Add(
                "@Name", SqlDbType.VarChar, 100).Value = request.Name;

            command.Parameters.Add(
                "@Email", SqlDbType.VarChar, 100).Value = request.Email;

            command.Parameters.Add(
                "@PasswordHash", SqlDbType.VarChar, 255).Value = passwordHash;

            command.Parameters.Add(
                "@PhoneNumber", SqlDbType.VarChar, 20).Value = request.PhoneNumber;

            command.Parameters.Add(
                "@CreatedBy", SqlDbType.VarChar, 20).Value = request.CreatedBy;

            // Step 5: Execute stored procedure
            object? result = await command.ExecuteScalarAsync();

            // Step 6: Return UserId
            return result == null || result == DBNull.Value
                ? 0
                : Convert.ToInt32(result);
        }



    }
}

 


