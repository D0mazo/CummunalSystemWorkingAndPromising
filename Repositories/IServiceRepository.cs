using CommunalSystem.Models;

namespace CommunalSystem.Repositories
{
    public interface IServiceRepository
    {
        int Save(string name);
        void Update(int serviceId, string name);
        void Delete(int serviceId);
        List<Service> GetAll();
        Service FindById(int serviceId);
    }
}