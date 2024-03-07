using Microsoft.EntityFrameworkCore;
using Quartz;
using WorkerService1.Contexts;
using WorkerService1.Models;

namespace WorkerService1.Jobs
{
    [DisallowConcurrentExecution]
    public class SyncJob(IDbContextFactory<ERPContext> ERPContextFactory, IDbContextFactory<KSOAContext> KSOAContextFactory) : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            using ERPContext _ERPContext = ERPContextFactory.CreateDbContext();
            using KSOAContext _KSOAContext = KSOAContextFactory.CreateDbContext();
            foreach (Product product in _ERPContext.Products)
            {
                if (_KSOAContext.hhyys.Any(x => x.skuCode.Equals(product.Id)))
                {
                    var _hhyy = _KSOAContext.hhyys.First(x => x.skuCode.Equals(product.Id));
                    _hhyy.factoryName = product.ProducerName;
                    _hhyy.productName = product.Name;
                    _hhyy.approvalNO = product.Approval;
                    _hhyy.url = product.Url;
                    _hhyy.units = product.Unit;
                    _hhyy.amount = product.StockAmount;
                    _hhyy.bigPackAmount = product.BigPack;
                    _hhyy.middlePackAmount = product.MidPack;
                    _hhyy.price = product.Price.ToString();
                    _hhyy.spec = product.Specs;
                    _hhyy.saleTag = product.Message;
                    _hhyy.noSellTip = product.IsBanned ? "超出经营范围" : string.Empty;
                    _hhyy.priceTip = "-";
                    _hhyy.isdeleted = 0;
                    _hhyy.status = 0;
                    if (product.Expiry.HasValue)
                    {
                        _hhyy.stockState = product.Expiry.ToString();
                    }
                    else
                    {
                        _hhyy.stockState = string.Empty;
                    }
                }
                else
                {
                    hhyy _hhyy = new()
                    {
                        factoryName = product.ProducerName,
                        productName = product.Name,
                        approvalNO = product.Approval,
                        url = product.Url,
                        units = product.Unit,
                        amount = product.StockAmount,
                        bigPackAmount = product.BigPack,
                        middlePackAmount = product.MidPack,
                        price = product.Price.ToString(),
                        skuCode = product.Id,
                        source = product.PlatformId,
                        spec = product.Specs,
                        saleTag = product.Message,
                        noSellTip = product.IsBanned ? "超出经营范围" : string.Empty,
                        priceTip = "-",
                        isdeleted = 0,
                        status = 0
                    };
                    if (product.Expiry.HasValue)
                    {
                        _hhyy.stockState = product.Expiry.ToString();
                    }
                    else
                    {
                        _hhyy.stockState = string.Empty;
                    }
                    _KSOAContext.hhyys.Add(_hhyy);
                }
                _KSOAContext.SaveChanges();
            }
            return Task.CompletedTask;
        }
    }
}
