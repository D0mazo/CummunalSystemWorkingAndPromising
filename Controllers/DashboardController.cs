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
            if (HttpContext.Session.GetString("Role") == null)
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("Role");
            var username = HttpContext.Session.GetString("Username");
            var user = _userRepo.FindByUsername(username);
            if (user == null) return RedirectToAction("Login", "Account");

            var data = user.GetDashboardData(); // Polymorphism

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
            return BadRequest("Invalid role");
        }

        // Admin: Community Operations
        [HttpPost]
        public IActionResult CreateCommunity(string name)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized();
            try
            {
                _adminService.CreateCommunity(name);
                TempData["Success"] = "Community created successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating community: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditCommunity(int communityId, string name)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized();
            try
            {
                _adminService.EditCommunity(communityId, name);
                TempData["Success"] = "Community updated successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating community: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        public IActionResult DeleteCommunity(int communityId)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized();
            try
            {
                _adminService.DeleteCommunity(communityId);
                TempData["Success"] = "Community deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting community: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        // Admin: Service Operations
        [HttpPost]
        public IActionResult CreateService(string name)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized();
            try
            {
                _adminService.CreateService(name);
                TempData["Success"] = "Service created successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating service: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditService(int serviceId, string name)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized();
            try
            {
                _adminService.EditService(serviceId, name);
                TempData["Success"] = "Service updated successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating service: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        public IActionResult DeleteService(int serviceId)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized();
            try
            {
                _adminService.DeleteService(serviceId);
                TempData["Success"] = "Service deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting service: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        // Admin: User Operations
        [HttpPost]
        public IActionResult CreateUser(string role, string firstName, string lastName, int? communityId = null)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized();
            try
            {
                _adminService.CreateUser(role, firstName, lastName, communityId);
                TempData["Success"] = $"User created successfully (Username: {firstName}, Password: {lastName})";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating user: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        public IActionResult DeleteUser(int userId)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized();
            try
            {
                _adminService.DeleteUser(userId);
                TempData["Success"] = "User deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting user: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        // Manager: Service Assignment Operations
        [HttpPost]
        public IActionResult AssignService(int communityId, int serviceId, decimal price)
        {
            if (HttpContext.Session.GetString("Role") != "manager") return Unauthorized();
            try
            {
                _managerService.AssignService(communityId, serviceId, price);
                TempData["Success"] = "Service assigned and price set successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error assigning service: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditPrice(int communityId, int serviceId, decimal price)
        {
            if (HttpContext.Session.GetString("Role") != "manager") return Unauthorized();
            try
            {
                _managerService.EditPrice(communityId, serviceId, price);
                TempData["Success"] = "Price updated successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating price: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        // Resident: View with Search
        [HttpPost]
        public IActionResult ViewServices(string search)
        {
            if (HttpContext.Session.GetString("Role") != "resident") return Unauthorized();
            try
            {
                var communityId = HttpContext.Session.GetInt32("CommunityId").Value;
                ViewBag.Services = _residentService.ViewServices(communityId, search);
                return View("Resident");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}