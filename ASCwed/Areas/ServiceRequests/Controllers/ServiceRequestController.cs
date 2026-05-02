using ASC.Business.Interfaces;
using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities.Extensions;
using ASCwed.Areas.ServiceRequests.Models;
using ASCwed.Cofiguration;
using ASCwed.Controllers;
using ASCwed.ServiceHub;
using ASCwed.Services.MasterData;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace ASCwed.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IServiceRequestMessageOperations _serviceRequestMessageOperations;
        private readonly IHubContext<ServiceMessagesHub> _hubContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOptions<ApplicationSettings> _options;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;

        public ServiceRequestController(
            IServiceRequestOperations serviceRequestOperations,
            IServiceRequestMessageOperations serviceRequestMessageOperations,
            IHubContext<ServiceMessagesHub> hubContext,
            UserManager<IdentityUser> userManager,
            IOptions<ApplicationSettings> options,
            IMapper mapper,
            IMasterDataCacheOperations masterData)
        {
            _serviceRequestOperations = serviceRequestOperations;
            _serviceRequestMessageOperations = serviceRequestMessageOperations;
            _hubContext = hubContext;
            _userManager = userManager;
            _options = options;
            _mapper = mapper;
            _masterData = masterData;
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            await PopulateMasterDataAsync();
            return View(new NewServiceRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel request)
        {
            NormalizeRequestedDate(request);

            if (!ModelState.IsValid)
            {
                await PopulateMasterDataAsync();
                return View(request);
            }

            var currentUser = User.ToCurrentUser();
            var serviceRequest = _mapper.Map<NewServiceRequestViewModel, ServiceRequest>(request);

            // Thiết lập khóa và trạng thái khởi tạo cho yêu cầu dịch vụ mới của khách hàng.
            serviceRequest.PartitionKey = currentUser.Email;
            serviceRequest.RowKey = Guid.NewGuid().ToString();
            serviceRequest.RequestedDate = request.RequestedDate;
            serviceRequest.Status = Status.New.ToString();
            serviceRequest.ServiceEngineer = string.Empty;
            serviceRequest.CreatedBy = currentUser.UserName;
            serviceRequest.UpdatedBy = currentUser.UserName;

            await _serviceRequestOperations.CreateServiceRequestAsync(serviceRequest);
            return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });
        }

        [HttpGet]
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var serviceRequest = await _serviceRequestOperations.GetServiceRequestAsync(partitionKey, rowKey);
            if (serviceRequest == null || !CanAccessServiceRequest(serviceRequest))
            {
                return NotFound();
            }

            var messages = await _serviceRequestMessageOperations.GetServiceRequestMessageAsync(rowKey);
            return View("ServiceRequestDetails", new ServiceRequestDetailsViewModel
            {
                ServiceRequest = serviceRequest,
                Messages = messages
            });
        }

        [HttpGet]
        public async Task<JsonResult> GetMessages(string rowKey)
        {
            var messages = await _serviceRequestMessageOperations.GetServiceRequestMessageAsync(rowKey);
            return Json(messages.OrderBy(message => message.MessageDate));
        }

        [HttpPost("/ServiceRequests/CreateMessage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMessage(string partitionKey, string rowKey, string message)
        {
            if (string.IsNullOrWhiteSpace(partitionKey)
                || string.IsNullOrWhiteSpace(rowKey)
                || string.IsNullOrWhiteSpace(message))
            {
                return Json(false);
            }

            var serviceRequest = await _serviceRequestOperations.GetServiceRequestAsync(partitionKey, rowKey);
            if (serviceRequest == null || !CanAccessServiceRequest(serviceRequest))
            {
                return Json(false);
            }

            var currentUser = User.ToCurrentUser();
            var serviceRequestMessage = new ServiceRequestMessage
            {
                PartitionKey = rowKey,
                RowKey = Guid.NewGuid().ToString(),
                FromDisplayName = currentUser.UserName,
                FromEmail = currentUser.Email,
                Message = message,
                MessageDate = DateTime.UtcNow,
                CreatedBy = currentUser.Email,
                UpdatedBy = currentUser.Email
            };

            await _serviceRequestMessageOperations.CreateServiceRequestMessageAsync(serviceRequestMessage);

            var recipients = await GetServiceRequestRecipientUserIdsAsync(serviceRequest);
            await _hubContext.Clients.Users(recipients).SendAsync("ReceiveMessage", serviceRequestMessage);

            return Json(true);
        }

        private async Task PopulateMasterDataAsync()
        {
            var masterData = await _masterData.GetMasterDataCacheAsync();

            ViewBag.VehicleTypes = masterData.Values
                .Where(item => item.PartitionKey == MasterKeys.VehicleType.ToString())
                .OrderBy(item => item.Name)
                .ToList();
            ViewBag.VehicleNames = masterData.Values
                .Where(item => item.PartitionKey == MasterKeys.VehicleName.ToString())
                .OrderBy(item => item.Name)
                .ToList();
        }

        private void NormalizeRequestedDate(NewServiceRequestViewModel request)
        {
            var requestedDateText = Request.Form[nameof(NewServiceRequestViewModel.RequestedDate)].ToString();
            if (string.IsNullOrWhiteSpace(requestedDateText))
            {
                return;
            }

            if (DateTime.TryParseExact(
                    requestedDateText,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var requestedDate))
            {
                request.RequestedDate = requestedDate;
                ModelState.Remove(nameof(NewServiceRequestViewModel.RequestedDate));
            }
        }

        private bool CanAccessServiceRequest(ServiceRequest serviceRequest)
        {
            var currentUser = User.ToCurrentUser();
            var adminEmail = _options.Value.AdminEmail ?? string.Empty;

            return IsSameEmail(currentUser.Email, adminEmail)
                || IsSameEmail(currentUser.Email, serviceRequest.PartitionKey)
                || IsSameEmail(currentUser.Email, serviceRequest.ServiceEngineer);
        }

        private async Task<List<string>> GetServiceRequestRecipientUserIdsAsync(ServiceRequest serviceRequest)
        {
            var recipientEmails = new[]
                {
                    serviceRequest.PartitionKey,
                    serviceRequest.ServiceEngineer,
                    _options.Value.AdminEmail
                }
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            var recipientUserIds = new List<string>();
            foreach (var email in recipientEmails)
            {
                var user = await _userManager.FindByEmailAsync(email!);
                if (user != null)
                {
                    recipientUserIds.Add(user.Id);
                }
            }

            return recipientUserIds;
        }

        private static bool IsSameEmail(string? firstEmail, string? secondEmail)
        {
            return !string.IsNullOrWhiteSpace(firstEmail)
                && !string.IsNullOrWhiteSpace(secondEmail)
                && string.Equals(firstEmail.Trim(), secondEmail.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
