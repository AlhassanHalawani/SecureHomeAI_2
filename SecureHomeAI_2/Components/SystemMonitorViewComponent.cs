using Microsoft.AspNetCore.Mvc;
using SecureHomeAI_2.Models;

namespace SecureHomeAI_2.Components
{
    public class SystemMonitorViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var systemStatus = new SystemStatus
            {
                Status = "SECURE",
                ConnectedDevices = new Random().Next(5, 15),
                SuspiciousActivities = new Random().Next(0, 3),
                LastChecked = DateTime.Now.ToString("HH:mm:ss")
            };

            return View(systemStatus);
        }
    }
}