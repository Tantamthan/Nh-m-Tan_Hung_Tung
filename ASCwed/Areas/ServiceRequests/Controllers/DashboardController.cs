using ASC.Business.Interfaces;
using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities.Extensions;
using ASCwed.Areas.ServiceRequests.Models;
using ASCwed.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ASCwed.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class DashboardController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;

        public DashboardController(IServiceRequestOperations serviceRequestOperations)
        {
            _serviceRequestOperations = serviceRequestOperations;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Danh sách trạng thái được truy vấn trên dashboard theo Lab 7.
            var status = new List<string>
            {
                Status.New.ToString(),
                Status.InProgress.ToString(),
                Status.Initiated.ToString(),
                Status.RequestForInformation.ToString()
            };

            var currentUser = User.ToCurrentUser();
            var serviceRequests = new List<ServiceRequest>();

            if (User.IsInRole(Roles.Admin.ToString()))
            {
                serviceRequests = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(DateTime.UtcNow.AddDays(-7), status);
            }
            else if (User.IsInRole(Roles.Engineer.ToString()))
            {
                serviceRequests = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(
                        DateTime.UtcNow.AddDays(-7),
                        status,
                        serviceEngineerEmail: currentUser.Email);
            }
            else
            {
                serviceRequests = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(
                        DateTime.UtcNow.AddYears(-1),
                        email: currentUser.Email);
            }

            return View(new DashboardViewModel
            {
                ServiceRequests = serviceRequests
                    .OrderByDescending(serviceRequest => serviceRequest.RequestedDate)
                    .ToList()
            });
        }
    }
}
