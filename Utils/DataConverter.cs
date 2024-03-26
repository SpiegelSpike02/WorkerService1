using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkerService1.Models;

namespace WorkerService1.Utils
{
    public class DataConverter
    {
        public static hhyy Convert(Product product)
        {
            var hhyy = new hhyy()
            {
                skuCode = product.Id,
                factoryName = product.ProducerName,
                productName = product.Name,
                approvalNO = product.Approval,
                url = product.Url,
                units = product.Unit,
                amount = product.StockAmount,
                bigPackAmount = product.BigPack,
                middlePackAmount = product.MidPack,
                price = product.Price.ToString(),
                spec = product.Specs,
                saleTag = product.Message,
                noSellTip = product.IsBanned ? "超出经营范围" : string.Empty,
                priceTip = "-",
                isdeleted = 0,
                status = 0
            };
            if (product.Expiry.HasValue)
            {
                hhyy.stockState = product.Expiry.ToString();
            }
            else
            {
                hhyy.stockState = string.Empty;
            }
            return hhyy;
        }
    }
}
