using CommunalSystem.Models;
using CommunalSystem.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommunalSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepo;

        public AccountController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
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
                TempData["Error"] = $"Prisijungimo klaida: {ex.Message}";
                return View();
            }
        }

        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                TempData["Success"] = "Atsijungta sėkmingai! Jūs nukreipiamas į prisijungimo puslapį.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Atsijungimo klaida: {ex.Message}";
                return RedirectToAction("Login");
            }
        }
    }
}