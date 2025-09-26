namespace CommunalSystem.Models
{
    public class Price
    {
        public int Id { get; private set; }
        public int CommunityId { get; private set; }
        public int ServiceId { get; private set; }
        public decimal Value { get; private set; } // Renamed to avoid conflict with class name

        public Price(int id, int communityId, int serviceId, decimal value)
        {
            Id = id;
            CommunityId = communityId;
            ServiceId = serviceId;
            Value = value;
        }

        public void SetValue(decimal value)
        {
            Value = value;
        }
    }
}