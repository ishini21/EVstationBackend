namespace EVOwnerManagement.API.Models
{
    public class OperatingHours
    {
        public string OpenTime { get; set; }  // e.g. "06:00"
        public string CloseTime { get; set; } // e.g. "22:00"
        public bool Is24Hours { get; set; } = false; // 24/7 operation flag
    }
}
