using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkerService1.Models
{
    public class Product
    {
        [Key]
        public required string Id { get; set; }

        public required string Name { get; set; }

        public required string Approval { get; set; }

        public required string Specs { get; set; }

        public required string ProducerName { get; set; }

        public int? BigPack { get; set; }

        public int? MidPack { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        public int? StockAmount { get; set; }

        public DateOnly? ProductDate { get; set; }

        public DateOnly? Expiry { get; set; }

        public string? Message { get; set; }

        public bool IsBanned { get; set; }

        public string? Unit { get; set; }

        public required string Url { get; set; }

        [ForeignKey("PlatformId")]
        public int PlatformId { get; set; }

        public Platform Platform { get; set; }
    }
}
