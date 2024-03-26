using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkerService1.Models
{
    public class hhyy
    {
        [Key]
        public string skuCode { get; set; }

        public string? productName { get; set; }

        public string? spec { get; set; }

        public string? factoryName { get; set; }

        public string? approvalNO { get; set; }

        public int? middlePackAmount { get; set; }

        public int? bigPackAmount { get; set; }

        public string? units { get; set; }

        public string? stockState { get; set; }

        public int? amount { get; set; }

        public string? price { get; set; }

        public string? priceTip { get; set; }

        public string? noSellTip { get; set; }

        public string? saleTag { get; set; }

        public int source { get; set; }

        public int status { get; set; }

        public int isdeleted { get; set; }

        public string? url { get; set; }
    }
}
