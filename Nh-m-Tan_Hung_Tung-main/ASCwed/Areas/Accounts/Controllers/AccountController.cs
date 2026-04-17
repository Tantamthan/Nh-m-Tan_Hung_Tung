using ASCwed.Areas.Accounts.Models;
using ASCwed.Controllers;
using ASCwed.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASCwed.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    public class AccountController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        // =========================================================
        // SERVICE ENGINEERS
        // =========================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ServiceEngineers()
        {
            var engineers = await _userManager.GetUsersInRoleAsync("Engineer");

            var engineerViewModels = engineers.Select(u => new ServiceEngineerUpdateViewModel
            {
                UserName = u.UserName!,
                Email = u.Email!,
                IsActive = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow
            }).OrderBy(e => e.UserName).ToList();

            var model = new ServiceEngineersViewModel
            {
                Registration = new ServiceEngineerRegistrationViewModel(),
                ServiceEngineers = engineerViewModels
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceEngineers(ServiceEngineersViewModel model)
        {
            // Re-load engineer list to always populate ServiceEngineers
            async Task<ServiceEngineersViewModel> RebuildViewModel()
            {
                var engineers = await _userManager.GetUsersInRoleAsync("Engineer");
                model.ServiceEngineers = engineers.Select(u => new ServiceEngineerUpdateViewModel
                {
                    UserName = u.UserName!,
                    Email = u.Email!,
                    IsActive = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow
                }).OrderBy(e => e.UserName).ToList();
                return model;
            }

            if (!ModelState.IsValid)
            {
                return View(await RebuildViewModel());
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Registration.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Registration.Email", "Email này đã được sử dụng.");
                return View(await RebuildViewModel());
            }

            var existingByName = await _userManager.FindByNameAsync(model.Registration.UserName);
            if (existingByName != null)
            {
                ModelState.AddModelError("Registration.UserName", "Tên đăng nhập này đã tồn tại.");
                return View(await RebuildViewModel());
            }

            // Create new engineer account
            var newUser = new IdentityUser
            {
                UserName = model.Registration.UserName,
                Email = model.Registration.Email,
                EmailConfirmed = true,
                LockoutEnabled = !model.Registration.IsActive
            };

            var result = await _userManager.CreateAsync(newUser, model.Registration.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(await RebuildViewModel());
            }

            await _userManager.AddToRoleAsync(newUser, "Engineer");
            await _userManager.AddClaimAsync(newUser, new System.Security.Claims.Claim(
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", model.Registration.Email));
            await _userManager.AddClaimAsync(newUser, new System.Security.Claims.Claim(
                "IsActive", model.Registration.IsActive ? "True" : "False"));

            // Send welcome email
            try
            {
                var emailBody = $@"
                    <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;border:1px solid #e0e0e0;border-radius:8px;'>
                        <h2 style='color:#1565C0;'>Chào mừng đến với ASC</h2>
                        <p>Xin chào <strong>{model.Registration.UserName}</strong>,</p>
                        <p>Tài khoản kỹ thuật viên của bạn đã được tạo thành công.</p>
                        <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
                            <tr><td style='padding:8px;background:#f5f5f5;font-weight:bold;'>Email:</td><td style='padding:8px;'>{model.Registration.Email}</td></tr>
                            <tr><td style='padding:8px;background:#f5f5f5;font-weight:bold;'>Tên đăng nhập:</td><td style='padding:8px;'>{model.Registration.UserName}</td></tr>
                            <tr><td style='padding:8px;background:#f5f5f5;font-weight:bold;'>Trạng thái:</td><td style='padding:8px;'>{(model.Registration.IsActive ? "✅ Đang hoạt động" : "❌ Bị khóa")}</td></tr>
                        </table>
                        <p style='color:#666;font-size:13px;'>Vui lòng đăng nhập và đổi mật khẩu ngay sau khi nhận được email này.</p>
                        <hr style='border:none;border-top:1px solid #e0e0e0;margin:16px 0;'/>
                        <p style='color:#999;font-size:12px;'>© Automobile Service Center</p>
                    </div>";

                await _emailSender.SendEmailAsync(
                    model.Registration.Email,
                    "Tài khoản kỹ thuật viên ASC đã được tạo",
                    emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Không thể gửi email chào mừng tới {Email}: {Error}", model.Registration.Email, ex.Message);
            }

            TempData["Success"] = $"Tài khoản kỹ thuật viên '{model.Registration.UserName}' đã được tạo thành công!";
            return RedirectToAction(nameof(ServiceEngineers));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateServiceEngineer(ServiceEngineerUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(ServiceEngineers));
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy nhân viên.";
                return RedirectToAction(nameof(ServiceEngineers));
            }

            var oldEmail = user.Email;
            bool emailChanged = !string.Equals(oldEmail, model.Email, StringComparison.OrdinalIgnoreCase);
            bool wasActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;
            bool statusChanged = wasActive != model.IsActive;

            // Update email
            if (emailChanged)
            {
                user.Email = model.Email;
                user.NormalizedEmail = model.Email.ToUpperInvariant();
                user.EmailConfirmed = true;
            }

            // Update active status via LockoutEnd
            if (model.IsActive)
            {
                user.LockoutEnd = null;
                user.LockoutEnabled = false;
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.MaxValue;
                user.LockoutEnabled = true;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(ServiceEngineers));
            }

            // Update IsActive claim
            var claims = await _userManager.GetClaimsAsync(user);
            var isActiveClaim = claims.FirstOrDefault(c => c.Type == "IsActive");
            if (isActiveClaim != null)
                await _userManager.RemoveClaimAsync(user, isActiveClaim);
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", model.IsActive ? "True" : "False"));

            // Send notification email
            try
            {
                var changes = new System.Text.StringBuilder();
                if (emailChanged)
                    changes.AppendLine($"<tr><td style='padding:8px;background:#f5f5f5;font-weight:bold;'>Email mới:</td><td style='padding:8px;'>{model.Email}</td></tr>");
                if (statusChanged)
                    changes.AppendLine($"<tr><td style='padding:8px;background:#f5f5f5;font-weight:bold;'>Trạng thái:</td><td style='padding:8px;'>{(model.IsActive ? "✅ Tài khoản được kích hoạt" : "❌ Tài khoản bị vô hiệu hóa")}</td></tr>");

                if (emailChanged || statusChanged)
                {
                    var emailBody = $@"
                        <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;border:1px solid #e0e0e0;border-radius:8px;'>
                            <h2 style='color:#1565C0;'>Thông báo cập nhật tài khoản</h2>
                            <p>Xin chào <strong>{user.UserName}</strong>,</p>
                            <p>Tài khoản của bạn vừa được cập nhật bởi Admin. Chi tiết thay đổi:</p>
                            <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
                                {changes}
                            </table>
                            <p style='color:#666;font-size:13px;'>Nếu bạn có thắc mắc vui lòng liên hệ quản trị viên hệ thống.</p>
                            <hr style='border:none;border-top:1px solid #e0e0e0;margin:16px 0;'/>
                            <p style='color:#999;font-size:12px;'>© Automobile Service Center</p>
                        </div>";

                    var targetEmail = emailChanged ? oldEmail : model.Email;
                    await _emailSender.SendEmailAsync(
                        targetEmail!,
                        "Thông báo: Tài khoản ASC của bạn đã được cập nhật",
                        emailBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Không thể gửi email thông báo tới {Email}: {Error}", model.Email, ex.Message);
            }

            TempData["Success"] = $"Tài khoản '{model.UserName}' đã được cập nhật thành công!";
            return RedirectToAction(nameof(ServiceEngineers));
        }

        // =========================================================
        // CUSTOMERS
        // =========================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            var customers = await _userManager.GetUsersInRoleAsync("User");

            var customerViewModels = customers.Select(u => new CustomerUpdateViewModel
            {
                UserName = u.UserName!,
                Email = u.Email!,
                IsActive = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow
            }).OrderBy(c => c.UserName).ToList();

            var model = new CustomersViewModel
            {
                Customers = customerViewModels
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCustomer(CustomerUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(Customers));
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Customers));
            }

            var oldEmail = user.Email;
            bool emailChanged = !string.Equals(oldEmail, model.Email, StringComparison.OrdinalIgnoreCase);
            bool wasActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;
            bool statusChanged = wasActive != model.IsActive;

            // Update email
            if (emailChanged)
            {
                user.Email = model.Email;
                user.NormalizedEmail = model.Email.ToUpperInvariant();
                user.EmailConfirmed = true;
            }

            // Update active status via LockoutEnd
            if (model.IsActive)
            {
                user.LockoutEnd = null;
                user.LockoutEnabled = false;
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.MaxValue;
                user.LockoutEnabled = true;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Customers));
            }

            // Update IsActive claim
            var claims = await _userManager.GetClaimsAsync(user);
            var isActiveClaim = claims.FirstOrDefault(c => c.Type == "IsActive");
            if (isActiveClaim != null)
                await _userManager.RemoveClaimAsync(user, isActiveClaim);
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", model.IsActive ? "True" : "False"));

            // Send notification email
            try
            {
                var changes = new System.Text.StringBuilder();
                if (emailChanged)
                    changes.AppendLine($"<tr><td style='padding:8px;background:#f5f5f5;font-weight:bold;'>Email mới:</td><td style='padding:8px;'>{model.Email}</td></tr>");
                if (statusChanged)
                    changes.AppendLine($"<tr><td style='padding:8px;background:#f5f5f5;font-weight:bold;'>Trạng thái:</td><td style='padding:8px;'>{(model.IsActive ? "✅ Tài khoản được kích hoạt" : "❌ Tài khoản bị vô hiệu hóa")}</td></tr>");

                if (emailChanged || statusChanged)
                {
                    var emailBody = $@"
                        <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;border:1px solid #e0e0e0;border-radius:8px;'>
                            <h2 style='color:#1565C0;'>Thông báo cập nhật tài khoản</h2>
                            <p>Xin chào <strong>{user.UserName}</strong>,</p>
                            <p>Tài khoản của bạn vừa được cập nhật bởi Admin. Chi tiết thay đổi:</p>
                            <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
                                {changes}
                            </table>
                            <p style='color:#666;font-size:13px;'>Nếu bạn có thắc mắc vui lòng liên hệ quản trị viên.</p>
                            <hr style='border:none;border-top:1px solid #e0e0e0;margin:16px 0;'/>
                            <p style='color:#999;font-size:12px;'>© Automobile Service Center</p>
                        </div>";

                    var targetEmail = emailChanged ? oldEmail : model.Email;
                    await _emailSender.SendEmailAsync(
                        targetEmail!,
                        "Thông báo: Tài khoản ASC của bạn đã được cập nhật",
                        emailBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Không thể gửi email thông báo tới {Email}: {Error}", model.Email, ex.Message);
            }

            TempData["Success"] = $"Tài khoản '{model.UserName}' đã được cập nhật thành công!";
            return RedirectToAction(nameof(Customers));
        }

        // =========================================================
        // PROFILE / EXTERNAL LOGIN
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng hiện tại.");
            }

            var model = new ProfileViewModel
            {
                UserName = user.UserName!,
                Email = user.Email!
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng hiện tại.");
            }

            // Always repopulate read-only fields
            model.Email = user.Email!;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username is taken
            var existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập này đã được sử dụng bởi người khác.");
                return View(model);
            }

            // Update username
            user.UserName = model.UserName;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                model.IsEditSuccess = true;
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public IActionResult ExternalLogin()
        {
            return View();
        }
    }
}
