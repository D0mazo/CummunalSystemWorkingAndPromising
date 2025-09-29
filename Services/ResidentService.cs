using CommunalSystem.Models;
using CommunalSystem.Repositories;

namespace CommunalSystem.Services
{
    public class ResidentService
    {
        private readonly ICommunityServiceRepository _communityServiceRepo;

        public ResidentService(ICommunityServiceRepository communityServiceRepo)
        {
            _communityServiceRepo = communityServiceRepo;
        }

        public List<ServicePrice> ViewServices(int communityId, string searchTerm = null)
        {
            return _communityServiceRepo.GetForCommunity(communityId, searchTerm);
        }
    }
}