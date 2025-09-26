using CommunalSystem.Models;

namespace CommunalSystem.Repositories
{
    public interface IUserRepository
    {
        User FindByUsername(string username);
        int Save(string firstName, string lastName, string role, int? communityId = null);
        void Delete(int userId);
        List<User> GetAll();
    }
}