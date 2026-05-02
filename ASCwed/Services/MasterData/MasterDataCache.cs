using ASC.Model.Models;

namespace ASCwed.Services.MasterData
{
    public class MasterDataCache
    {
        public List<MasterDataKey> Keys { get; set; } = [];

        public List<MasterDataValue> Values { get; set; } = [];
    }
}
