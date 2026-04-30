namespace ASCwed.Areas.Configuration.Models
{
    public class MasterKeysViewModel
    {
        public List<MasterDataKeyViewModel>? MasterKeys { get; set; }

        public MasterDataKeyViewModel MasterKeyInContext { get; set; } = new();

        public bool IsEdit { get; set; }
    }
}
