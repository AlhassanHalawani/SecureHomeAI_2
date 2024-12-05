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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Email and password are required";
                return RedirectToAction("Login");
            }

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
        [ValidateAntiForgeryToken]
        public IActionResult Register(string FullName, string Email, string Password, string confirmPassword)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(FullName) ||
                    string.IsNullOrEmpty(Email) ||
                    string.IsNullOrEmpty(Password) ||
                    string.IsNullOrEmpty(confirmPassword))
                {
                    TempData["Error"] = "All fields are required";
                    return RedirectToAction("Register");
                }

                // Validate password length
                if (Password.Length < 8)
                {
                    TempData["Error"] = "Password must be at least 8 characters long";
                    return RedirectToAction("Register");
                }

                // Check password match
                if (Password != confirmPassword)
                {
                    TempData["Error"] = "Passwords do not match";
                    return RedirectToAction("Register");
                }

                // Check if email already exists
                if (_context.Users.Any(u => u.Email == Email))
                {
                    TempData["Error"] = "Email already registered";
                    return RedirectToAction("Register");
                }

                // Create new user
                var user = new User
                {
                    FullName = FullName,
                    Email = Email,
                    Password = Password // In production, hash the password
                };

                // Add user to database
                _context.Users.Add(user);
                _context.SaveChanges();

                _logger.LogInformation($"New user registered: {Email}");
                TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                TempData["Error"] = "An error occurred during registration";
                return RedirectToAction("Register");
            }
        }

        // GET: Home/UserManagement
        public IActionResult UserManagement(string searchQuery = null)
        {
            // Check if user is authenticated
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login");
            }

            try
            {
                IQueryable<User> query = _context.Users;

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    query = query.Where(u =>
                        u.FullName.Contains(searchQuery) ||
                        u.Email.Contains(searchQuery));
                }

                var users = query.ToList();
                ViewBag.SearchQuery = searchQuery;
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserManagement: {ex.Message}");
                TempData["Error"] = "Error loading users";
                return View(new List<User>());
            }
        }

        // GET: Home/AddUser
        [HttpGet]
        public IActionResult AddUser()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // POST: Home/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(User user, string confirmPassword)
        {
            if (user.Password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                TempData["Error"] = "Passwords do not match";
                return View(user);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (_context.Users.Any(u => u.Email == user.Email))
                    {
                        ModelState.AddModelError("Email", "Email already exists");
                        TempData["Error"] = "Email already exists";
                        return View(user);
                    }

                    _context.Users.Add(user);
                    _context.SaveChanges();
                    TempData["Success"] = "User added successfully";
                    return RedirectToAction("UserManagement");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error adding user: {ex.Message}");
                    ModelState.AddModelError("", "Error occurred while saving the user");
                    return View(user);
                }
            }
            return View(user);
        }

        // GET: Home/EditUser/5
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login");
            }

            try
            {
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("UserManagement");
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user for edit: {ex.Message}");
                TempData["Error"] = "Error retrieving user";
                return RedirectToAction("UserManagement");
            }
        }

        // POST: Home/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User updatedUser, string confirmPassword)
        {
            if (updatedUser.Password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                TempData["Error"] = "Passwords do not match";
                return View(updatedUser);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = _context.Users.Find(updatedUser.Id);
                    if (existingUser == null)
                    {
                        TempData["Error"] = "User not found";
                        return RedirectToAction("UserManagement");
                    }

                    // Check if email is being changed and if it's already in use
                    if (existingUser.Email != updatedUser.Email &&
                        _context.Users.Any(u => u.Email == updatedUser.Email))
                    {
                        ModelState.AddModelError("Email", "Email already exists");
                        TempData["Error"] = "Email already exists";
                        return View(updatedUser);
                    }

                    existingUser.FullName = updatedUser.FullName;
                    existingUser.Email = updatedUser.Email;
                    existingUser.Password = updatedUser.Password;

                    _context.Update(existingUser);
                    _context.SaveChanges();
                    TempData["Success"] = "User updated successfully";
                    return RedirectToAction("UserManagement");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating user: {ex.Message}");
                    ModelState.AddModelError("", "Error occurred while updating the user");
                    return View(updatedUser);
                }
            }
            return View(updatedUser);
        }

        // POST: Home/DeleteUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("UserManagement");
                }

                _context.Users.Remove(user);
                _context.SaveChanges();
                TempData["Success"] = "User deleted successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                TempData["Error"] = "Error occurred while deleting the user";
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