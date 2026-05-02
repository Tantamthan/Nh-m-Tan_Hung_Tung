namespace ASCwed.Areas.Configuration.Models
{
    public class MasterValuesViewModel
    {
        public List<MasterDataValueViewModel>? MasterValues { get; set; }

        public MasterDataValueViewModel MasterValueInContext { get; set; } = new();

        public bool IsEdit { get; set; }
    }
}
