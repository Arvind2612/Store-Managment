using System.ComponentModel.DataAnnotations;

namespace Store_Managment.Models
{
    public class Changepassword
    {
        public string Email { get; set; } = string.Empty;
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}

