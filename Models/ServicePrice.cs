namespace CommunalSystem.Models
{
    public class ServicePrice
    {
        public int Id { get; private set; }
        public int CommunityId { get; private set; }
        public int ServiceId { get; private set; }
        public decimal Price { get; private set; } 
        public string ServiceName { get; private set; }

        public ServicePrice(int id, int communityId, int serviceId, decimal price, string serviceName)
        {
            Id = id;
            CommunityId = communityId;
            ServiceId = serviceId;
            Price = price;
            ServiceName = serviceName;
        }
    }
}