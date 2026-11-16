namespace Store_Managment.Models
{
    public class UserLoggedin
    {

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}


