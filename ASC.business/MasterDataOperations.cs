using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;

namespace ASC.Business
{
    public class MasterDataOperations : IMasterDataOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public MasterDataOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MasterDataKey>> GetAllMasterKeysAsync()
        {
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();
            return masterKeys
                .Where(masterKey => !masterKey.IsDeleted)
                .OrderBy(masterKey => masterKey.PartitionKey)
                .ThenBy(masterKey => masterKey.Name)
                .ToList();
        }

        public async Task<List<MasterDataKey>> GetMasterKeyByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new List<MasterDataKey>();
            }

            // Master key trong LAB được lưu theo PartitionKey, tên hàm giữ ý nghĩa nghiệp vụ.
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>()
                .FindAllByPartitionKeyAsync(name.Trim());

            return masterKeys
                .Where(masterKey => !masterKey.IsDeleted)
                .ToList();
        }

        public async Task<bool> InsertMasterKeyAsync(MasterDataKey key)
        {
            NormalizeMasterKey(key);

            await _unitOfWork.Repository<MasterDataKey>().AddAsync(key);
            _unitOfWork.CommitTransaction();

            return true;
        }

        public async Task<bool> UpdateMasterKeyAsync(string originalPartitionKey, MasterDataKey key)
        {
            var masterKey = await _unitOfWork.Repository<MasterDataKey>()
                .FindAsync(originalPartitionKey, key.RowKey);

            if (masterKey == null)
            {
                return false;
            }

            // Không đổi khóa chính trong bước update để tránh EF tracking lỗi composite key.
            masterKey.Name = key.Name;
            masterKey.IsActive = key.IsActive;
            masterKey.IsDeleted = key.IsDeleted;
            masterKey.UpdatedBy = key.UpdatedBy;

            _unitOfWork.Repository<MasterDataKey>().Update(masterKey);
            _unitOfWork.CommitTransaction();

            return true;
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesByKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return new List<MasterDataValue>();
            }

            var masterValues = await _unitOfWork.Repository<MasterDataValue>()
                .FindAllByPartitionKeyAsync(key.Trim());

            return masterValues
                .Where(masterValue => !masterValue.IsDeleted)
                .OrderBy(masterValue => masterValue.Name)
                .ToList();
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesAsync()
        {
            var masterValues = await _unitOfWork.Repository<MasterDataValue>().FindAllAsync();
            return masterValues
                .Where(masterValue => !masterValue.IsDeleted)
                .OrderBy(masterValue => masterValue.PartitionKey)
                .ThenBy(masterValue => masterValue.Name)
                .ToList();
        }

        public async Task<MasterDataValue?> GetMasterValueByNameAsync(string key, string name)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var masterValues = await GetAllMasterValuesByKeyAsync(key);
            return masterValues.FirstOrDefault(masterValue =>
                string.Equals(masterValue.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> InsertMasterValueAsync(MasterDataValue value)
        {
            NormalizeMasterValue(value);

            await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
            _unitOfWork.CommitTransaction();

            return true;
        }

        public async Task<bool> UpdateMasterValueAsync(string originalPartitionKey, string originalRowKey, MasterDataValue value)
        {
            var masterValue = await _unitOfWork.Repository<MasterDataValue>()
                .FindAsync(originalPartitionKey, originalRowKey);

            if (masterValue == null)
            {
                return false;
            }

            // Không đổi khóa chính trong bước update để dữ liệu ổn định với composite key.
            masterValue.Name = value.Name;
            masterValue.IsActive = value.IsActive;
            masterValue.IsDeleted = value.IsDeleted;
            masterValue.UpdatedBy = value.UpdatedBy;

            _unitOfWork.Repository<MasterDataValue>().Update(masterValue);
            _unitOfWork.CommitTransaction();

            return true;
        }

        public async Task<bool> UploadBulkMasterData(List<MasterDataValue> values)
        {
            if (values == null || values.Count == 0)
            {
                return false;
            }

            var existingKeys = await GetAllMasterKeysAsync();
            var ensuredMasterKeys = existingKeys
                .Select(masterKey => masterKey.PartitionKey)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var existingValues = await GetAllMasterValuesAsync();
            var valueLookup = existingValues
                .GroupBy(masterValue => CreateValueLookupKey(masterValue.PartitionKey, masterValue.Name))
                .ToDictionary(group => group.Key, group => group.First());

            foreach (var value in values)
            {
                NormalizeMasterValue(value);

                if (!ensuredMasterKeys.Contains(value.PartitionKey))
                {
                    await _unitOfWork.Repository<MasterDataKey>().AddAsync(new MasterDataKey
                    {
                        PartitionKey = value.PartitionKey,
                        RowKey = Guid.NewGuid().ToString(),
                        Name = value.PartitionKey,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = value.CreatedBy,
                        UpdatedBy = value.UpdatedBy
                    });

                    ensuredMasterKeys.Add(value.PartitionKey);
                }

                var lookupKey = CreateValueLookupKey(value.PartitionKey, value.Name);
                if (!valueLookup.TryGetValue(lookupKey, out var masterValue))
                {
                    await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
                    valueLookup[lookupKey] = value;
                }
                else
                {
                    masterValue.Name = value.Name;
                    masterValue.IsActive = value.IsActive;
                    masterValue.IsDeleted = value.IsDeleted;
                    masterValue.UpdatedBy = value.UpdatedBy;
                    _unitOfWork.Repository<MasterDataValue>().Update(masterValue);
                }
            }

            _unitOfWork.CommitTransaction();
            return true;
        }

        private static string CreateValueLookupKey(string partitionKey, string name)
        {
            return $"{partitionKey.Trim()}::{name.Trim()}".ToUpperInvariant();
        }

        private static void NormalizeMasterKey(MasterDataKey key)
        {
            key.PartitionKey = key.PartitionKey?.Trim() ?? string.Empty;
            key.RowKey = string.IsNullOrWhiteSpace(key.RowKey) ? Guid.NewGuid().ToString() : key.RowKey.Trim();
            key.Name = key.Name?.Trim() ?? string.Empty;
            key.CreatedBy ??= string.Empty;
            key.UpdatedBy ??= key.CreatedBy;
        }

        private static void NormalizeMasterValue(MasterDataValue value)
        {
            value.PartitionKey = value.PartitionKey?.Trim() ?? string.Empty;
            value.RowKey = string.IsNullOrWhiteSpace(value.RowKey) ? Guid.NewGuid().ToString() : value.RowKey.Trim();
            value.Name = value.Name?.Trim() ?? string.Empty;
            value.CreatedBy ??= string.Empty;
            value.UpdatedBy ??= value.CreatedBy;
        }
    }
}
