using System.ComponentModel.DataAnnotations;

namespace Store_Managment.Models
{
    public class Logout
    {
            [Required]
            public string RefreshToken { get; set; } = string.Empty;
        
    }

}

