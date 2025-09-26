using CommunalSystem.Models;

namespace CommunalSystem.Repositories
{
    public interface ICommunityRepository
    {
        int Save(string name);
        void Update(int communityId, string name);
        void Delete(int communityId);
        List<Community> GetAll();
        Community FindById(int communityId);
    }
}