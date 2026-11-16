//using System.ComponentModel.DataAnnotations;

//namespace Store_Managment.Models
//{
//    public class Register
//    {

//        [Required]
//        public string Name { get; set; } = string.Empty;

//        [Required]
//        [EmailAddress]
//        public string Email { get; set; } = string.Empty;

//        [Required]
//        [MinLength(8)]
//        public string Password { get; set; } = string.Empty;

//        [Required]
//        public string PhoneNumber { get; set; } = string.Empty;

//    }

//}

using System.ComponentModel.DataAnnotations;

namespace Store_Managment.Models
{
    public class Register
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        public string  CreatedBy { get; set; } =  "Self-Registeration";
    }
}
