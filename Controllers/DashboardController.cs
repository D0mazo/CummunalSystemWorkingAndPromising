using CommunalSystem.Models;
using CommunalSystem.Repositories;
using CommunalSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            try
            {
                var role = HttpContext.Session.GetString("Role");
                if (string.IsNullOrEmpty(role))
                    return RedirectToAction("Login", "Account");

                var username = HttpContext.Session.GetString("Username");
                var user = _userRepo.FindByUsername(username);
                if (user == null)
                {
                    TempData["Error"] = "Vartotojas nerastas. Prašome prisijungti iš naujo.";
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.Message = role switch
                {
                    "admin" => "Šis puslapis skirtas administratoriui valdyti bendrijas, paslaugas ir vartotojus.",
                    "manager" => "Šis puslapis skirtas vadybininkui priskirti paslaugas ir nustatyti kainas.",
                    "resident" => "Šis puslapis skirtas gyventojui peržiūrėti savo bendrijos paslaugas ir kainas.",
                    _ => "Netinkamas vaidmuo."
                };

                return role switch
                {
                    "admin" => AdminView(),
                    "manager" => ManagerView(),
                    "resident" => ResidentView(),
                    _ => BadRequest("Netinkamas vaidmuo.")
                };
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida įkeliant dashboard: {ex.Message}";
                return RedirectToAction("Login", "Account");
            }
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
                TempData["Error"] = "Jūsų bendrijos ID nerastas. Prašome prisijungti iš naujo.";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Services = _residentService.ViewServices(communityId.Value);
            return View("Resident");
        }

        // ===========================
        // Admin: Community Operations
        // ===========================
        [HttpPost]
        public IActionResult CreateCommunity(string name) => AdminAction(() =>
        {
            _adminService.CreateCommunity(name);
            TempData["Success"] = "Nauja bendrija pridėta sėkmingai!";
        }, "Bendrijos kūrimo klaida");

        [HttpPost]
        public IActionResult EditCommunity(int communityId, string name) => AdminAction(() =>
        {
            _adminService.EditCommunity(communityId, name);
            TempData["Success"] = "Bendrija atnaujinta sėkmingai!";
        }, "Bendrijos atnaujinimo klaida");

        public IActionResult DeleteCommunity(int communityId) => AdminAction(() =>
        {
            _adminService.DeleteCommunity(communityId);
            TempData["Success"] = "Bendrija ištrinta sėkmingai!";
        }, "Bendrijos trinimo klaida");

        // ===========================
        // Admin: Service Operations
        // ===========================
        [HttpPost]
        public IActionResult CreateService(string name) => AdminAction(() =>
        {
            _adminService.CreateService(name);
            TempData["Success"] = "Nauja paslauga pridėta sėkmingai!";
        }, "Paslaugos kūrimo klaida");

        [HttpPost]
        public IActionResult EditService(int serviceId, string name) => AdminAction(() =>
        {
            _adminService.EditService(serviceId, name);
            TempData["Success"] = "Paslauga atnaujinta sėkmingai!";
        }, "Paslaugos atnaujinimo klaida");

        public IActionResult DeleteService(int serviceId) => AdminAction(() =>
        {
            _adminService.DeleteService(serviceId);
            TempData["Success"] = "Paslauga ištrinta sėkmingai!";
        }, "Paslaugos trinimo klaida");

        // ===========================
        // Admin: User Operations
        // ===========================
        [HttpPost]
        public IActionResult CreateUser(string role, string firstName, string lastName, int? communityId = null) => AdminAction(() =>
        {
            _adminService.CreateUser(role, firstName, lastName, communityId);
            TempData["Success"] = $"Naujas vartotojas pridėtas sėkmingai! (Vartotojo vardas: {firstName}, Slaptažodis: {lastName})";
        }, "Vartotojo kūrimo klaida");

        public IActionResult DeleteUser(int userId) => AdminAction(() =>
        {
            _adminService.DeleteUser(userId);
            TempData["Success"] = "Vartotojas ištrintas sėkmingai!";
        }, "Vartotojo trinimo klaida");

        // ===========================
        // Manager: Service Assignment
        // ===========================
        [HttpPost]
        public IActionResult AssignService(int communityId, int serviceId, decimal price) => ManagerAction(() =>
        {
            _managerService.AssignService(communityId, serviceId, price);
            TempData["Success"] = "Paslauga priskirta ir kaina nustatyta sėkmingai!";
        }, "Paslaugos priskyrimo klaida");

        [HttpPost]
        public IActionResult EditPrice(int communityId, int serviceId, decimal price) => ManagerAction(() =>
        {
            _managerService.EditPrice(communityId, serviceId, price);
            TempData["Success"] = "Kaina atnaujinta sėkmingai!";
        }, "Kainos atnaujinimo klaida");

        // ===========================
        // Resident: View Services
        // ===========================
        [HttpPost]
        public IActionResult ViewServices(string search)
        {
            if (!IsRole("resident")) return Unauthorized("Neturite gyventojo teisių.");
            try
            {
                var communityId = HttpContext.Session.GetInt32("CommunityId").Value;
                ViewBag.Services = _residentService.ViewServices(communityId, search);
                TempData["Message"] = "Paslaugų sąrašas atnaujintas pagal jūsų paiešką.";
                return View("Resident");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida peržiūrint paslaugas: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // ===========================
        // Helper Methods
        // ===========================
        private IActionResult AdminAction(Action action, string errorMessage)
        {
            if (!IsRole("admin")) return Unauthorized("Neturite administratoriaus teisių.");
            try
            {
                action();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"{errorMessage}: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        private IActionResult ManagerAction(Action action, string errorMessage)
        {
            if (!IsRole("manager")) return Unauthorized("Neturite vadybininko teisių.");
            try
            {
                action();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"{errorMessage}: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        private bool IsRole(string role) => HttpContext.Session.GetString("Role") == role;
    }
}
