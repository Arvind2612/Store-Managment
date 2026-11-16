using System.ComponentModel.DataAnnotations;

namespace Store_Managment.Models
{
    public class ForgetPass
    {
 
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        
    }

}

