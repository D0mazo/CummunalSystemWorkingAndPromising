using System.Collections.Generic;

namespace CommunalSystem.Models
{
    public class ResidentDashboardViewModel
    {
        public string CommunityName { get; set; }
        public List<ServicePrice> Services { get; set; } = new List<ServicePrice>();
        public string Message { get; set; }
        public string TempSuccess { get; set; }
        public string TempError { get; set; }
    }
}
