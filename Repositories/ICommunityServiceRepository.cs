using CommunalSystem.Models;

namespace CommunalSystem.Repositories
{
    public interface ICommunityServiceRepository
    {
        int AssignAndSetPrice(int communityId, int serviceId, decimal price);
        void UpdatePrice(int communityId, int serviceId, decimal price);
        List<ServicePrice> GetForCommunity(int communityId, string searchTerm = null);
    }
}