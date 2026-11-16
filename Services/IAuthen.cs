 
using Store_Managment.Models;

namespace Store_Managment.Services
{
    public interface IAuthService
    {
        // Login
        Task<LoginResponse?> LoginAsync(LoginRequest request);

        // Logout
        Task<bool> LogoutAsync(Logout request);

        // Refresh Token
        Task<LoginResponse?> RefreshTokenAsync(
            RefreshTokenRequest request);

        // Forgot Password
        Task<bool> ForgotPasswordAsync(ForgetPass request);

        // Reset Password
        Task<bool> ResetPasswordAsync(ResetPassword request);

        // Change Password
        Task<bool> ChangePasswordAsync(Changepassword request);

        // Get Current User
        Task<UserLoggedin?> GetCurrentUserAsync(int userId);

        // Get Permissions
        Task<List<UserPermission>> GetPermissionsAsync(int userId);

        // Register
        Task<int> RegisterAsync(Register request);
    }
}
