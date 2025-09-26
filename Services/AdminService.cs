using CommunalSystem.Repositories;

namespace CommunalSystem.Services
{
    public class AdminService
    {
        private readonly IUserRepository _userRepo;
        private readonly ICommunityRepository _communityRepo;
        private readonly IServiceRepository _serviceRepo;

        public AdminService(IUserRepository userRepo, ICommunityRepository communityRepo, IServiceRepository serviceRepo)
        {
            _userRepo = userRepo;
            _communityRepo = communityRepo;
            _serviceRepo = serviceRepo;
        }

        public int CreateUser(string role, string firstName, string lastName, int? communityId = null)
        {
            if (role != "manager" && role != "resident")
                throw new ArgumentException("Invalid role");
            return _userRepo.Save(firstName, lastName, role, communityId);
        }

        public void DeleteUser(int userId)
        {
            _userRepo.Delete(userId);
        }

        public int CreateCommunity(string name)
        {
            return _communityRepo.Save(name);
        }

        public void EditCommunity(int communityId, string name)
        {
            _communityRepo.Update(communityId, name);
        }

        public void DeleteCommunity(int communityId)
        {
            _communityRepo.Delete(communityId);
        }

        public int CreateService(string name)
        {
            return _serviceRepo.Save(name);
        }

        public void EditService(int serviceId, string name)
        {
            _serviceRepo.Update(serviceId, name);
        }

        public void DeleteService(int serviceId)
        {
            _serviceRepo.Delete(serviceId);
        }
    }
}