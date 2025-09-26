using CommunalSystem.Repositories;

namespace CommunalSystem.Services
{
    public class ManagerService
    {
        private readonly ICommunityServiceRepository _communityServiceRepo;

        public ManagerService(ICommunityServiceRepository communityServiceRepo)
        {
            _communityServiceRepo = communityServiceRepo;
        }

        public int AssignService(int communityId, int serviceId, decimal price)
        {
            return _communityServiceRepo.AssignAndSetPrice(communityId, serviceId, price);
        }

        public void EditPrice(int communityId, int serviceId, decimal price)
        {
            _communityServiceRepo.UpdatePrice(communityId, serviceId, price);
        }
    }
}