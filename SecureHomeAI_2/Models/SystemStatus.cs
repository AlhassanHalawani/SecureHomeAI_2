namespace SecureHomeAI_2.Models
{
    public class SystemStatus
    {
        public string Status { get; set; }
        public int ConnectedDevices { get; set; }
        public int SuspiciousActivities { get; set; }
        public string LastChecked { get; set; }
    }

}
