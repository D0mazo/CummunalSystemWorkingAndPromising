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
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _userRepo.FindByUsername(username);
            if (user != null && user.Password == password)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("Username", user.Username);
                if (user.CommunityId.HasValue)
                    HttpContext.Session.SetInt32("CommunityId", user.CommunityId.Value);
                TempData["Success"] = "Login successful";
                return RedirectToAction("Index", "Dashboard");
            }
            TempData["Error"] = "Invalid credentials";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Logged out";
            return RedirectToAction("Login");
        }
    }
}