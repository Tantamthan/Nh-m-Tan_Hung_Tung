using ASC.Model.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASCwed.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        // 1. Khai báo các DbSet ở cấp độ class
        public virtual DbSet<MasterDataKey> MasterDataKeys { get; set; }
        public virtual DbSet<MasterDataValue> MasterDataValues { get; set; }
        public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }
         
        // 2. Giữ lại một hàm khởi tạo duy nhất
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.Migrate();
        }

        // 3. Cấu hình khóa chính
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MasterDataKey>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });

            builder.Entity<MasterDataValue>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });

            builder.Entity<ServiceRequest>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });

            base.OnModelCreating(builder);
        }
    }
}