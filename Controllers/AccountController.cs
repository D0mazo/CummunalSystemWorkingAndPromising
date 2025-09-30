using CommunalSystem.Models;
using CommunalSystem.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommunalSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserRepository userRepo, ILogger<AccountController> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Message = "Šis puslapis skirtas prisijungti prie komunalinių paslaugų sistemos. Įveskite savo vartotojo vardą ir slaptažodį.";
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            try
            {
                var user = _userRepo.FindByUsername(username);
                if (user == null || user.Password != password)
                {
                    TempData["Error"] = "Neteisingi prisijungimo duomenys. Patikrinkite vartotojo vardą ir slaptažodį.";
                    return View();
                }

                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("Username", user.Username);

                if (user.CommunityId.HasValue)
                    HttpContext.Session.SetInt32("CommunityId", user.CommunityId.Value);

                TempData["Success"] = "Prisijungta sėkmingai! Jūs nukreipiamas į pagrindinį puslapį.";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Username}", username);
                TempData["Error"] = $"Prisijungimo klaida: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear session data
            HttpContext.Session.Clear();

            _logger.LogInformation("User logged out.");
            TempData["Success"] = "Atsijungėte sėkmingai.";

            // Redirect back to Login page
            return RedirectToAction("Login", "Account");
        }

    }
}
