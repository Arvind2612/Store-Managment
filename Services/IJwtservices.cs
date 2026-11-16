namespace Store_Managment.Services
{
 
    public interface IJwtService
    {
        string GenerateAccessToken(
            int userId,
            string userName,
            string email,
            List<string> roles);

        string GenerateRefreshToken();

        string HashToken(string token);
    }
}
