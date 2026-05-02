using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Utilities.Extensions;
using ASCwed.Areas.Configuration.Models;
using ASCwed.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace ASCwed.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : BaseController
    {
        private readonly IMasterDataOperations _masterData;
        private readonly IMapper _mapper;

        public MasterDataController(IMasterDataOperations masterData, IMapper mapper)
        {
            _masterData = masterData;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            var masterKeysViewModel = _mapper.Map<List<MasterDataKey>, List<MasterDataKeyViewModel>>(masterKeys);

            // Lưu danh sách master key vào session để các view/AJAX có thể dùng lại cùng request flow.
            HttpContext.Session.SetObjectAsJson("MasterKeys", masterKeysViewModel);

            return View(new MasterKeysViewModel
            {
                MasterKeys = masterKeysViewModel,
                IsEdit = false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(bool isEdit, MasterDataKeyViewModel masterKey)
        {
            if (!ModelState.IsValid)
            {
                return Json("Error");
            }

            var currentUser = User.ToCurrentUser();
            var masterDataKey = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(masterKey);
            masterDataKey.PartitionKey = string.IsNullOrWhiteSpace(masterDataKey.PartitionKey)
                ? masterDataKey.Name.Trim()
                : masterDataKey.PartitionKey.Trim();
            masterDataKey.Name = masterDataKey.Name.Trim();
            masterDataKey.UpdatedBy = currentUser.UserName;

            if (isEdit)
            {
                await _masterData.UpdateMasterKeyAsync(masterDataKey.PartitionKey, masterDataKey);
            }
            else
            {
                // Tạo RowKey mới cho master key được thêm từ màn hình quản trị.
                masterDataKey.RowKey = Guid.NewGuid().ToString();
                masterDataKey.CreatedBy = currentUser.UserName;
                await _masterData.InsertMasterKeyAsync(masterDataKey);
            }

            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            ViewBag.MasterKeys = await _masterData.GetAllMasterKeysAsync();

            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),
                IsEdit = false
            });
        }

        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string key)
        {
            var masterValues = await _masterData.GetAllMasterValuesByKeyAsync(key);
            var masterValuesViewModel = _mapper.Map<List<MasterDataValue>, List<MasterDataValueViewModel>>(masterValues);

            return Json(new { data = masterValuesViewModel });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel masterValue)
        {
            if (!ModelState.IsValid)
            {
                return Json("Error");
            }

            var currentUser = User.ToCurrentUser();
            var masterDataValue = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(masterValue);
            masterDataValue.PartitionKey = masterDataValue.PartitionKey.Trim();
            masterDataValue.Name = masterDataValue.Name.Trim();
            masterDataValue.UpdatedBy = currentUser.UserName;

            if (isEdit)
            {
                await _masterData.UpdateMasterValueAsync(masterDataValue.PartitionKey, masterDataValue.RowKey, masterDataValue);
            }
            else
            {
                // Tạo RowKey mới cho master value được thêm từ màn hình quản trị.
                masterDataValue.RowKey = Guid.NewGuid().ToString();
                masterDataValue.CreatedBy = currentUser.UserName;
                await _masterData.InsertMasterValueAsync(masterDataValue);
            }

            return Json(true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel()
        {
            var files = Request.Form.Files;
            if (!files.Any())
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }

            var excelFile = files.First();
            if (excelFile.Length <= 0)
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }

            try
            {
                var masterData = await ParseMasterDataExcel(excelFile);
                var currentUser = User.ToCurrentUser();

                foreach (var value in masterData)
                {
                    value.CreatedBy = currentUser.UserName;
                    value.UpdatedBy = currentUser.UserName;
                }

                var result = await _masterData.UploadBulkMasterData(masterData);
                return Json(new { Success = result });
            }
            catch (InvalidDataException ex)
            {
                return Json(new { Error = true, Text = ex.Message });
            }
            catch (Exception ex)
            {
                // Trả lỗi dạng JSON để màn hình import hiển thị được nguyên nhân thay vì rơi vào AJAX error chung.
                return Json(new { Error = true, Text = $"Cannot import Excel file. {ex.Message}" });
            }
        }

        private static async Task<List<MasterDataValue>> ParseMasterDataExcel(IFormFile excelFile)
        {
            var masterValueList = new List<MasterDataValue>();

            await using var memoryStream = new MemoryStream();
            await excelFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // EPPlus 8 yêu cầu khai báo license trước khi đọc workbook.
            ExcelPackage.License.SetNonCommercialOrganization("Agrivity");

            using var excelPackage = new ExcelPackage(memoryStream);
            var worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null || worksheet.Dimension == null)
            {
                throw new InvalidDataException("Excel file không có dữ liệu.");
            }

            var rowCount = worksheet.Dimension.Rows;

            // Dòng 1 là header; dữ liệu bắt đầu từ dòng 2 theo xác nhận của yêu cầu.
            for (var row = 2; row <= rowCount; row++)
            {
                var masterKey = worksheet.Cells[row, 1].Text.Trim();
                var masterValue = worksheet.Cells[row, 2].Text.Trim();
                var isActiveText = worksheet.Cells[row, 3].Text.Trim();

                if (string.IsNullOrWhiteSpace(masterKey)
                    && string.IsNullOrWhiteSpace(masterValue)
                    && string.IsNullOrWhiteSpace(isActiveText))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(masterKey) || string.IsNullOrWhiteSpace(masterValue))
                {
                    throw new InvalidDataException($"Dòng {row}: MasterKey và MasterValue không được để trống.");
                }

                if (!string.Equals(isActiveText, "TRUE", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(isActiveText, "FALSE", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"Dòng {row}: IsActive phải là TRUE hoặc FALSE.");
                }

                masterValueList.Add(new MasterDataValue
                {
                    PartitionKey = masterKey,
                    RowKey = Guid.NewGuid().ToString(),
                    Name = masterValue,
                    IsActive = string.Equals(isActiveText, "TRUE", StringComparison.OrdinalIgnoreCase)
                });
            }

            return masterValueList;
        }
    }
}
