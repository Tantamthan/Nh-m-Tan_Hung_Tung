using ASC.Model.BaseTypes;

namespace ASC.Model.Models
{
    public class OnlineUser : BaseEntity
    {
        public OnlineUser()
        {
        }

        public OnlineUser(string email)
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = email;
        }
    }
}
