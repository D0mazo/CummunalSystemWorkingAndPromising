namespace CommunalSystem.Models
{
    public class Community
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public Community(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public void SetName(string name)
        {
            Name = name;
        }
    }
}