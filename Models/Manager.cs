namespace CommunalSystem.Models
{
    public class Manager : User
    {
        public Manager(int id, string username, string password, string role, string firstName, string lastName, int? communityId)
            : base(id, username, password, role, firstName, lastName, communityId) { }

        public override object GetDashboardData()
        {
            return new { Message = "Manager dashboard: Assign services and set prices" };
        }
    }
}