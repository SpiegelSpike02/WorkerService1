using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkerService1.Models
{
    public class Product
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Approval { get; set; }

        public string Specs { get; set; }

        public string ProducerName { get; set; }

        public int? BigPack { get; set; }

        public int? MidPack { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        public int? StockAmount { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ProductDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Expiry { get; set; }

        public string? SaleTip { get; set; }

        public string? SellTip { get; set; }

        public string? Unit { get; set; }

        public string Url { get; set; }

        [ForeignKey("PlatformId")]
        public int PlatformId { get; set; }

        public Platform Platform { get; set; }
    }
}
