using R.Model;
using SM.Model;

namespace I.Service
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsers();
        Task<User> GetUserById(int id);
        Task<int> CreateUser(User user);
        Task<bool> UpdateUser(int id, User user);
        Task<bool> AssignRoles(int userId, List<int> roleId);
        Task<List<Role>> GetUserRoles(int userId);
        Task<bool> DeleteUser(int id);
    }
}
