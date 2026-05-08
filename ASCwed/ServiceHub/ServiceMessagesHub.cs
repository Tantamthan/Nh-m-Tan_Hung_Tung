using ASC.Business.Interfaces;
using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities.Extensions;
using ASCwed.Cofiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace ASCwed.ServiceHub
{
    [Authorize]
    public class ServiceMessagesHub : Hub
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IOnlineUsersOperations _onlineUsersOperations;
        private readonly IOptions<ApplicationSettings> _options;

        public ServiceMessagesHub(
            IServiceRequestOperations serviceRequestOperations,
            IOnlineUsersOperations onlineUsersOperations,
            IOptions<ApplicationSettings> options)
        {
            _serviceRequestOperations = serviceRequestOperations;
            _onlineUsersOperations = onlineUsersOperations;
            _options = options;
        }

        public override async Task OnConnectedAsync()
        {
            var connectionInfo = GetConnectionInfo();
            if (!connectionInfo.IsValid)
            {
                Context.Abort();
                return;
            }

            var serviceRequest = await _serviceRequestOperations.GetServiceRequestAsync(
                connectionInfo.PartitionKey,
                connectionInfo.RowKey);
            var currentUser = Context.User?.ToCurrentUser();

            if (serviceRequest == null || currentUser == null || !CanAccessServiceRequest(currentUser.Email, serviceRequest))
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GetServiceRequestGroupName(connectionInfo.PartitionKey, connectionInfo.RowKey));
            await _onlineUsersOperations.CreateOnlineUserAsync(currentUser.Email);
            await UpdateServiceRequestClientsAsync(serviceRequest);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionInfo = GetConnectionInfo();
            var currentUser = Context.User?.ToCurrentUser();

            if (connectionInfo.IsValid && currentUser != null)
            {
                var serviceRequest = await _serviceRequestOperations.GetServiceRequestAsync(
                    connectionInfo.PartitionKey,
                    connectionInfo.RowKey);

                if (serviceRequest != null && CanAccessServiceRequest(currentUser.Email, serviceRequest))
                {
                    await _onlineUsersOperations.DeleteOnlineUserAsync(currentUser.Email);
                    await UpdateServiceRequestClientsAsync(serviceRequest);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public static string GetServiceRequestGroupName(string partitionKey, string rowKey)
        {
            return $"service-request:{partitionKey.Trim().ToUpperInvariant()}:{rowKey.Trim().ToUpperInvariant()}";
        }

        private async Task UpdateServiceRequestClientsAsync(ServiceRequest serviceRequest)
        {
            var adminEmail = _options.Value.AdminEmail ?? string.Empty;
            var engineerEmail = string.IsNullOrWhiteSpace(serviceRequest.ServiceEngineer)
                ? _options.Value.EngineerEmail ?? string.Empty
                : serviceRequest.ServiceEngineer;
            var customerEmail = serviceRequest.PartitionKey;

            var isAdminOnline = await _onlineUsersOperations.GetOnlineUserAsync(adminEmail);
            var isEngineerOnline = !string.IsNullOrWhiteSpace(engineerEmail)
                && await _onlineUsersOperations.GetOnlineUserAsync(engineerEmail);
            var isCustomerOnline = await _onlineUsersOperations.GetOnlineUserAsync(customerEmail);

            // Cập nhật trạng thái online cho các browser đang mở đúng service request này.
            await Clients.Group(GetServiceRequestGroupName(serviceRequest.PartitionKey, serviceRequest.RowKey))
                .SendAsync("UpdateOnlineStatus", new
                {
                    isAd = isAdminOnline,
                    isSe = isEngineerOnline,
                    isCu = isCustomerOnline
                });
        }

        private bool CanAccessServiceRequest(string currentEmail, ServiceRequest serviceRequest)
        {
            var adminEmail = _options.Value.AdminEmail ?? string.Empty;

            return IsSameEmail(currentEmail, adminEmail)
                || IsSameEmail(currentEmail, serviceRequest.PartitionKey)
                || IsSameEmail(currentEmail, serviceRequest.ServiceEngineer)
                || (Context.User?.IsInRole(Roles.Engineer.ToString()) == true
                    && string.IsNullOrWhiteSpace(serviceRequest.ServiceEngineer));
        }

        private static bool IsSameEmail(string? firstEmail, string? secondEmail)
        {
            return !string.IsNullOrWhiteSpace(firstEmail)
                && !string.IsNullOrWhiteSpace(secondEmail)
                && string.Equals(firstEmail.Trim(), secondEmail.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private ServiceRequestConnectionInfo GetConnectionInfo()
        {
            var request = Context.GetHttpContext()?.Request;
            var partitionKey = request?.Query["partitionKey"].ToString() ?? string.Empty;
            var rowKey = request?.Query["rowKey"].ToString() ?? string.Empty;

            return new ServiceRequestConnectionInfo(partitionKey, rowKey);
        }

        private sealed record ServiceRequestConnectionInfo(string PartitionKey, string RowKey)
        {
            public bool IsValid => !string.IsNullOrWhiteSpace(PartitionKey) && !string.IsNullOrWhiteSpace(RowKey);
        }
    }
}
