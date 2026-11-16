namespace Store_Managment.Models
{
    public class LoginResponse
    {

            public int UserId { get; set; }

            public string UserName { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string AccessToken { get; set; } = string.Empty;

            public string RefreshToken { get; set; } = string.Empty;

            public DateTime AccessTokenExpiresAt { get; set; }

            public DateTime RefreshTokenExpiresAt { get; set; }
        }
    }


