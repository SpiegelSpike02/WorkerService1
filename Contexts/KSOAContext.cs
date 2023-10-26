using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkerService1.Models;

namespace WorkerService1.Contexts
{
    public class KSOAContext : DbContext
    {
        public virtual DbSet<hhyy> hhyys { get; set; }

        public KSOAContext()
        {
        }

        public KSOAContext(DbContextOptions<KSOAContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=windows-79th3sp\\sqlexpress2012;Initial Catalog=ksoa;User ID=sa;Password=jxy-15805249769;TrustServerCertificate=true;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity(delegate (EntityTypeBuilder<hhyy> entity)
            {
                entity.HasKey((hhyy e) => new { e.skuCode, e.source }).HasName("PK__hhyy__EE963A744235D556");
                entity.ToTable("hhyy");
                entity.Property((hhyy e) => e.skuCode).HasMaxLength(255).IsUnicode(unicode: false)
                    .HasComment("货号");
                entity.Property((hhyy e) => e.source).HasComment("产品来源,1代表淮海 2代表 九州通");
                entity.Property((hhyy e) => e.amount).HasComment("库存");
                entity.Property((hhyy e) => e.approvalNO).HasMaxLength(4000).IsUnicode(unicode: false)
                    .HasComment("批准文号");
                entity.Property((hhyy e) => e.bigPackAmount).HasComment("大包装");
                entity.Property((hhyy e) => e.factoryName).HasMaxLength(4000).IsUnicode(unicode: false)
                    .HasComment("生产厂家");
                entity.Property((hhyy e) => e.isdeleted).HasComment("0代表未删除，-1代表已经删除");
                entity.Property((hhyy e) => e.middlePackAmount).HasComment("中包装");
                entity.Property((hhyy e) => e.noSellTip).HasMaxLength(4000).IsUnicode(unicode: false)
                    .HasComment("备注");
                entity.Property((hhyy e) => e.price).HasMaxLength(4000).IsUnicode(unicode: false)
                    .HasComment("价格");
                entity.Property((hhyy e) => e.priceTip).HasMaxLength(4000).IsUnicode(unicode: false)
                    .HasComment("零售价");
                entity.Property((hhyy e) => e.productName).HasMaxLength(4000).IsUnicode(unicode: false)
                    .HasComment("商品名称");
                entity.Property((hhyy e) => e.saleTag).HasMaxLength(4000).IsUnicode(unicode: false)
                    .HasComment("促销信息");
                entity.Property((hhyy e) => e.spec).HasMaxLength(4000).IsUnicode(unicode: false)
                    .HasComment("规格");
                entity.Property((hhyy e) => e.status).HasComment("0代表未上架，1代表已经上架");
                entity.Property((hhyy e) => e.stockState).HasMaxLength(4000).IsUnicode(unicode: false)
                    .HasComment("效期");
                entity.Property((hhyy e) => e.units).HasMaxLength(4000).IsUnicode(unicode: false)
                    .HasComment("单位");
                entity.Property((hhyy e) => e.url).HasMaxLength(4000).IsUnicode(unicode: false);
            });
        }
    }

}
