namespace CommunalSystem.Models
{
    public class Admin : User
    {
        public Admin(int id, string username, string password, string role, string firstName, string lastName, int? communityId)
            : base(id, username, password, role, firstName, lastName, communityId) { }

        public override object GetDashboardData()
        {
            return new { Message = "" };
        }
    }
}