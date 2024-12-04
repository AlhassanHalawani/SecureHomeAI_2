using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SecureHomeAI_2.Data; // Add reference to the Data namespace
using SecureHomeAI_2.Models;
using System.Linq;

namespace SecureHomeAI_2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Dashboard
        public IActionResult Index()
        {
            var status = new SystemStatus
            {
                Status = "SECURE",
                ConnectedDevices = 12,
                SuspiciousActivities = 0,
                LastChecked = "Just Now"
            };

            var users = _context.Users.ToList(); // Retrieve users from the database

            // Ensure the second item in the tuple is cast to IEnumerable<User>
            var model = Tuple.Create(status, (IEnumerable<User>)users);
            return View(model);
        }


        // User Management Page (Search and List Users)
        public IActionResult UserManagement(string searchQuery = null)
        {
            var users = string.IsNullOrEmpty(searchQuery)
                ? _context.Users.ToList()
                : _context.Users.Where(u => u.FullName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.SearchQuery = searchQuery;
            return View(users);
        }

        // Add User (GET)
        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        // Add User (POST)
        [HttpPost]
        public IActionResult AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("UserManagement");
        }

        // Edit User (GET)
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // Edit User (POST)
        [HttpPost]
        public IActionResult EditUser(User updatedUser)
        {
            var user = _context.Users.Find(updatedUser.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = updatedUser.FullName;
            user.Email = updatedUser.Email;
            user.Password = updatedUser.Password;
            _context.SaveChanges();

            return RedirectToAction("UserManagement");
        }

        // Delete User
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("UserManagement");
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
