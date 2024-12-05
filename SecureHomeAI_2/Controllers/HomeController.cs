using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SecureHomeAI_2.Data;
using SecureHomeAI_2.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;

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

        // GET: Home/Index
        public IActionResult Index()
        {
            // Check if user is authenticated
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login");
            }

            var status = new SystemStatus
            {
                Status = "SECURE",
                ConnectedDevices = 12,
                SuspiciousActivities = 0,
                LastChecked = "Just Now"
            };

            var users = _context.Users.ToList();
            var model = Tuple.Create(status, (IEnumerable<User>)users);

            // Add user name to ViewBag for display
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(model);
        }

        // GET: Home/Login
        public IActionResult Login()
        {
            // If user is already logged in, redirect to Index
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // POST: Home/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user != null && user.Password == password) // In production, use proper password hashing
            {
                // Set Session Variables
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserEmail", user.Email);

                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation($"User {user.Email} logged in at {DateTime.UtcNow}");
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Invalid login credentials";
            return RedirectToAction("Login");
        }

        // GET: Home/Logout
        public async Task<IActionResult> Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Sign out the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        // GET: Home/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Home/Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    TempData["Error"] = "Email already registered";
                    return View(user);
                }

                // In production, hash the password before saving
                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            return View(user);
        }

        // GET: Home/UserManagement
        public IActionResult UserManagement(string searchQuery = null)
        {
            // Check if user is authenticated
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login");
            }

            var users = string.IsNullOrEmpty(searchQuery)
                ? _context.Users.ToList()
                : _context.Users.Where(u => u.FullName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.SearchQuery = searchQuery;
            return View(users);
        }

        // GET: Home/AddUser
        [HttpGet]
        public IActionResult AddUser()
        {
            // Check if user is authenticated
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // POST: Home/AddUser
        [HttpPost]
        public IActionResult AddUser(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                TempData["Success"] = "User added successfully";
                return RedirectToAction("UserManagement");
            }
            return View(user);
        }

        // GET: Home/EditUser/5
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            // Check if user is authenticated
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Home/EditUser
        [HttpPost]
        public IActionResult EditUser(User updatedUser)
        {
            if (ModelState.IsValid)
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

                TempData["Success"] = "User updated successfully";
                return RedirectToAction("UserManagement");
            }
            return View(updatedUser);
        }

        // POST: Home/DeleteUser/5
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                TempData["Success"] = "User deleted successfully";
            }
            return RedirectToAction("UserManagement");
        }

        // GET: Home/About
        public IActionResult About()
        {
            return View();
        }

        // GET: Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: Home/GetSystemStatus
        [HttpGet]
        public IActionResult GetSystemStatus()
        {
            var status = new SystemStatus
            {
                Status = "SECURE",
                ConnectedDevices = new Random().Next(5, 15),
                SuspiciousActivities = new Random().Next(0, 3),
                LastChecked = DateTime.Now.ToString("HH:mm:ss")
            };
            return Json(status);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}