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

        public DashboardController(IUserRepository userRepo, ICommunityRepository communityRepo, IServiceRepository serviceRepo,
            AdminService adminService, ManagerService managerService, ResidentService residentService)
        {
            _userRepo = userRepo;
            _communityRepo = communityRepo;
            _serviceRepo = serviceRepo;
            _adminService = adminService;
            _managerService = managerService;
            _residentService = residentService;
        }

        public IActionResult Index()
        {
            try
            {
                if (HttpContext.Session.GetString("Role") == null)
                    return RedirectToAction("Login", "Account");

                var role = HttpContext.Session.GetString("Role");
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

                if (role == "admin")
                {
                    ViewBag.Communities = _communityRepo.GetAll();
                    ViewBag.Services = _serviceRepo.GetAll();
                    ViewBag.Users = _userRepo.GetAll();
                    return View("Admin");
                }
                else if (role == "manager")
                {
                    ViewBag.Communities = _communityRepo.GetAll();
                    ViewBag.Services = _serviceRepo.GetAll();
                    return View("Manager");
                }
                else if (role == "resident")
                {
                    var communityId = HttpContext.Session.GetInt32("CommunityId").Value;
                    ViewBag.Services = _residentService.ViewServices(communityId);
                    return View("Resident");
                }
                return BadRequest("Netinkamas vaidmuo.");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida įkeliant dashboard: {ex.Message}";
                return RedirectToAction("Login", "Account");
            }
        }

        // Admin: Community Operations
        [HttpPost]
        public IActionResult CreateCommunity(string name)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized("Neturite administratoriaus teisių.");
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Bendrijos pavadinimas negali būti tuščias.");
                _adminService.CreateCommunity(name);
                TempData["Success"] = "Nauja bendrija pridėta sėkmingai!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida kuriant bendriją: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditCommunity(int communityId, string name)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized("Neturite administratoriaus teisių.");
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Bendrijos pavadinimas negali būti tuščias.");
                _adminService.EditCommunity(communityId, name);
                TempData["Success"] = "Bendrija atnaujinta sėkmingai!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida atnaujinant bendriją: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        public IActionResult DeleteCommunity(int communityId)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized("Neturite administratoriaus teisių.");
            try
            {
                _adminService.DeleteCommunity(communityId);
                TempData["Success"] = "Bendrija ištrinta sėkmingai!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida trinant bendriją: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        // Admin: Service Operations
        [HttpPost]
        public IActionResult CreateService(string name)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized("Neturite administratoriaus teisių.");
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Paslaugos pavadinimas negali būti tuščias.");
                _adminService.CreateService(name);
                TempData["Success"] = "Nauja paslauga pridėta sėkmingai!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida kuriant paslaugą: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditService(int serviceId, string name)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized("Neturite administratoriaus teisių.");
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Paslaugos pavadinimas negali būti tuščias.");
                _adminService.EditService(serviceId, name);
                TempData["Success"] = "Paslauga atnaujinta sėkmingai!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida atnaujinant paslaugą: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        public IActionResult DeleteService(int serviceId)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized("Neturite administratoriaus teisių.");
            try
            {
                var service = _serviceRepo.FindById(serviceId);
                if (service == null)
                    throw new InvalidOperationException("Paslauga nerasta.");
                _adminService.DeleteService(serviceId);
                TempData["Success"] = "Paslauga ištrinta sėkmingai!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida trinant paslaugą: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        // Admin: User Operations
        [HttpPost]
        public IActionResult CreateUser(string role, string firstName, string lastName, int? communityId = null)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized("Neturite administratoriaus teisių.");
            try
            {
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                    throw new ArgumentException("Vardas ir pavardė negali būti tušti.");
                if (!new[] { "manager", "resident" }.Contains(role))
                    throw new ArgumentException("Netinkamas vaidmuo.");
                _adminService.CreateUser(role, firstName, lastName, communityId);
                TempData["Success"] = $"Naujas vartotojas pridėtas sėkmingai! (Vartotojo vardas: {firstName}, Slaptažodis: {lastName})";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida kuriant vartotoją: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        public IActionResult DeleteUser(int userId)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized("Neturite administratoriaus teisių.");
            try
            {
                var user = _userRepo.GetAll().FirstOrDefault(u => u.Id == userId);
                if (user == null)
                    throw new InvalidOperationException("Vartotojas nerastas.");
                _adminService.DeleteUser(userId);
                TempData["Success"] = "Vartotojas ištrintas sėkmingai!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida trinant vartotoją: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        // Manager: Service Assignment Operations
        [HttpPost]
        public IActionResult AssignService(int communityId, int serviceId, decimal price)
        {
            if (HttpContext.Session.GetString("Role") != "manager") return Unauthorized("Neturite vadybininko teisių.");
            try
            {
                var service = _serviceRepo.FindById(serviceId);
                if (service == null)
                    throw new InvalidOperationException("Paslauga nerasta.");
                var community = _communityRepo.FindById(communityId);
                if (community == null)
                    throw new InvalidOperationException("Bendrija nerasta.");
                _managerService.AssignService(communityId, serviceId, price);
                TempData["Success"] = "Paslauga priskirta ir kaina nustatyta sėkmingai!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida priskiriant paslaugą: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditPrice(int communityId, int serviceId, decimal price)
        {
            if (HttpContext.Session.GetString("Role") != "manager") return Unauthorized("Neturite vadybininko teisių.");
            try
            {
                _managerService.EditPrice(communityId, serviceId, price);
                TempData["Success"] = "Kaina atnaujinta sėkmingai!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Klaida atnaujinant kainą: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        // Resident: View with Search
        [HttpPost]
        public IActionResult ViewServices(string search)
        {
            if (HttpContext.Session.GetString("Role") != "resident") return Unauthorized("Neturite gyventojo teisių.");
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
    }
}