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

            var data = user.GetDashboardData();

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

        [HttpPost]
        public IActionResult CreateCommunity(string name)
        {
            if (HttpContext.Session.GetString("Role") != "admin") return Unauthorized();
            try
            {
                _adminService.CreateCommunity(name);
                TempData["Success"] = "Community created";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AssignService(int communityId, int serviceId, decimal price)
        {
            if (HttpContext.Session.GetString("Role") != "manager") return Unauthorized();
            try
            {
                _managerService.AssignService(communityId, serviceId, price);
                TempData["Success"] = "Service assigned and price set";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

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