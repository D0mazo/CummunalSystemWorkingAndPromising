namespace CommunalSystem.Models
{
    public abstract class User
    {
        public int Id { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Role { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public int? CommunityId { get; private set; }

        protected User(int id, string username, string password, string role, string firstName, string lastName, int? communityId)
        {
            Id = id;
            Username = username;
            Password = password;
            Role = role;
            FirstName = firstName;
            LastName = lastName;
            CommunityId = communityId;
        }

        // Polymorphism: Overridden in subclasses
        public abstract object GetDashboardData();
    }
}