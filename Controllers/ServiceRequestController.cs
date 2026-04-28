using Microsoft.AspNetCore.Mvc;
using Lab8_PhamVanTung_2324801030079.Services;
using Lab8_PhamVanTung_2324801030079.Models;

namespace Lab8_PhamVanTung_2324801030079.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly IServiceRequestOperations _service;

        public ServiceRequestController(IServiceRequestOperations service)
        {
            _service = service;
        }

        // GET: hiển thị form
        public IActionResult Create()
        {
            return View();
        }

        // POST: submit form
        [HttpPost]
        public async Task<IActionResult> Create(ServiceRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new ServiceRequest
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority
            };

            await _service.CreateAsync(request);

            TempData["Success"] = "Tạo yêu cầu thành công!";

            return RedirectToAction("Create");
        }
        public async Task<IActionResult> Dashboard()
        {
            var data = await _service.GetAllAsync();

            var vm = new DashboardViewModel
            {
                ServiceRequests = data.ToList()
            };

            return View(vm);
        }
    }
}