using System;
using System.Collections.Generic;

namespace Lab8_PhamVanTung_2324801030079.Models.Cache
{
    public class MasterDataCache
    {
        public string CacheKey { get; set; } = "ASCInstanceMasterDataCache";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Dictionary<string, object?> Data { get; set; } = new();
    }
}