using CommunalSystem.Models;
using MySqlConnector;

namespace CommunalSystem.Repositories
{
    public interface ICommunityServiceRepository
    {
        bool CommunityPriceExists(int communityId, int serviceId);
        int AssignAndSetPrice(int communityId, int serviceId, decimal price);
        void UpdatePrice(int communityId, int serviceId, decimal price);
        List<ServicePrice> GetForCommunity(int communityId, string searchTerm = null);
    }
}