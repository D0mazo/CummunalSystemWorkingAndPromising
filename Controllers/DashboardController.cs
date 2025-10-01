using CommunalSystem.Models;
using CommunalSystem.Repositories;
using CommunalSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CommunalSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly ICommunityRepository _communityRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly AdminService _adminService;
        private readonly ManagerService _managerService;
        private readonly ResidentService _residentService;

        public DashboardController(
            IUserRepository userRepo,
            ICommunityRepository communityRepo,
            IServiceRepository serviceRepo,
            AdminService adminService,
            ManagerService managerService,
            ResidentService residentService)
        {
            _userRepo = userRepo;
            _communityRepo = communityRepo;
            _serviceRepo = serviceRepo;
            _adminService = adminService;
            _managerService = managerService;
            _residentService = residentService;
        }

        // ===========================
        // Main Dashboard
        // ===========================
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            return role switch
            {
                "admin" => AdminView(),
                "manager" => ManagerView(),
                "resident" => ResidentView(),
                _ => BadRequest("Netinkamas vaidmuo.")
            };
        }

        // ===========================
        // Admin Views
        // ===========================
        private IActionResult AdminView()
        {
            ViewBag.Communities = _communityRepo.GetAll();
            ViewBag.Services = _serviceRepo.GetAll();
            ViewBag.Users = _userRepo.GetAll();
            return View("Admin");
        }

        private IActionResult ManagerView()
        {
            ViewBag.Communities = _communityRepo.GetAll();
            ViewBag.Services = _serviceRepo.GetAll();
            return View("Manager");
        }

        private IActionResult ResidentView()
        {
            var communityId = HttpContext.Session.GetInt32("CommunityId");
            if (!communityId.HasValue)
            {
                TempData["Error"] = "Jūsų bendrijos ID nerastas.";
                return RedirectToAction("Login", "Account");
            }

            var community = _communityRepo.FindById(communityId.Value);
            var services = _residentService.ViewServices(communityId.Value);

            var model = new ResidentDashboardViewModel
            {
                CommunityName = community?.Name ?? "Nerasta bendrija",
                Services = services,
                Message = "Šis puslapis skirtas gyventojui peržiūrėti savo bendrijos paslaugas ir kainas.",
                TempSuccess = TempData["Success"] as string,
                TempError = TempData["Error"] as string
            };

            return View("Resident", model);
        }

        // ===========================
        // Resident: Search Services
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ViewServices(string search)
        {
            var communityId = HttpContext.Session.GetInt32("CommunityId");
            if (!communityId.HasValue) return RedirectToAction("Index");

            var community = _communityRepo.FindById(communityId.Value);
            var services = _residentService.ViewServices(communityId.Value, search);

            var model = new ResidentDashboardViewModel
            {
                CommunityName = community?.Name ?? "Nerasta bendrija",
                Services = services,
                Message = "Paslaugų sąrašas atnaujintas pagal jūsų paiešką.",
                TempSuccess = TempData["Success"] as string,
                TempError = TempData["Error"] as string
            };

            return View("Resident", model);
        }

        // ===========================
        // Admin: Community Operations
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCommunity(string name) => AdminAction(() =>
        {
            _adminService.CreateCommunity(name);
            TempData["Success"] = "Nauja bendrija pridėta sėkmingai!";
        }, "Bendrijos kūrimo klaida");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCommunity(int communityId, string name) => AdminAction(() =>
        {
            _adminService.EditCommunity(communityId, name);
            TempData["Success"] = "Bendrija atnaujinta sėkmingai!";
        }, "Bendrijos atnaujinimo klaida");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCommunity(int communityId) => AdminAction(() =>
        {
            _adminService.DeleteCommunity(communityId);
            TempData["Success"] = "Bendrija ištrinta sėkmingai!";
        }, "Bendrijos trinimo klaida");

        // ===========================
        // Admin: Service Operations
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateService(string name) => AdminAction(() =>
        {
            _adminService.CreateService(name);
            TempData["Success"] = "Nauja paslauga pridėta sėkmingai!";
        }, "Paslaugos kūrimo klaida");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditService(int serviceId, string name) => AdminAction(() =>
        {
            _adminService.EditService(serviceId, name);
            TempData["Success"] = "Paslauga atnaujinta sėkmingai!";
        }, "Paslaugos atnaujinimo klaida");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteService(int serviceId) => AdminAction(() =>
        {
            _adminService.DeleteService(serviceId);
            TempData["Success"] = "Paslauga ištrinta sėkmingai!";
        }, "Paslaugos trinimo klaida");

        // ===========================
        // Admin: User Operations
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(string role, string firstName, string lastName, int? communityId = null) => AdminAction(() =>
        {
            _adminService.CreateUser(role, firstName, lastName, communityId);
            TempData["Success"] = $"Naujas vartotojas pridėtas sėkmingai! (Vartotojo vardas: {firstName}, Slaptažodis: {lastName})";
        }, "Vartotojo kūrimo klaida");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int userId) => AdminAction(() =>
        {
            _adminService.DeleteUser(userId);
            TempData["Success"] = "Vartotojas ištrintas sėkmingai!";
        }, "Vartotojo trinimo klaida");

        // ===========================
        // Manager: Service Assignment
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignService(int communityId, int serviceId, decimal price) => ManagerAction(() =>
        {
            _managerService.AssignService(communityId, serviceId, price);
            TempData["Success"] = "Paslauga priskirta ir kaina nustatyta sėkmingai!";
        }, "Paslaugos priskyrimo klaida");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPrice(int communityId, int serviceId, decimal price) => ManagerAction(() =>
        {
            _managerService.EditPrice(communityId, serviceId, price);
            TempData["Success"] = "Kaina atnaujinta sėkmingai!";
        }, "Kainos atnaujinimo klaida");

        // ===========================
        // Helper Methods
        // ===========================
        private IActionResult AdminAction(Action action, string errorMessage)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized();
            try { action(); }
            catch (Exception ex) { TempData["Error"] = $"{errorMessage}: {ex.Message}"; }
            return RedirectToAction("Index");
        }

        private IActionResult ManagerAction(Action action, string errorMessage)
        {
            if (HttpContext.Session.GetString("Role") != "manager") return Unauthorized();
            try { action(); }
            catch (Exception ex) { TempData["Error"] = $"{errorMessage}: {ex.Message}"; }
            return RedirectToAction("Index");
        }
    }
}
