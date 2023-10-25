// WorkerService1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// WorkerService1.Jobs.AYJob
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Quartz;
using WorkerService1.Contexts;
using WorkerService1.Models;

namespace WorkerService1.Jobs
{
    [DisallowConcurrentExecution]
    public class AYJob : IJob
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IDbContextFactory<ERPContext> _ERPContextFactory;

        public AYJob(IHttpClientFactory httpClientFactory, IDbContextFactory<ERPContext> ERPContextFactory)
        {
            _httpClientFactory = httpClientFactory;
            _ERPContextFactory = ERPContextFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using ERPContext _context = await _ERPContextFactory.CreateDbContextAsync();
                Platform AY = new()
                {
                    Id = 7,
                    Name = "澳洋"
                };
                if (!await _context.Platforms.ContainsAsync(AY))
                {
                    await _context.Platforms.AddAsync(AY);
                    await _context.SaveChangesAsync();
                }

                using HttpClient client = _httpClientFactory.CreateClient("AY");
                FormUrlEncodedContent form = new(new KeyValuePair<string, string>[4]
                {
                new("validateToken", "eecbebd1e028941d6e1966be53f8afc2"),
                new("username", "ay012392"),
                new("password", "jxy15805249769"),
                new("validateCode", "")
                });
                await client.PostAsync("front/loginapi/login", form);
                JsonNode jsonNode = JsonNode.Parse(await (await client.GetAsync("front/goods/api/goodslist?keyword=&storeId=&brandId=&gcId=&goodsSpec=&arrivalCycle=&sortField=&sortOrder=&dosageForm=&maximumPrice=&minimumPrice=&pageSize=12&pageNo=1&activityId=&prdId=")).Content.ReadAsStringAsync());
                jsonNode = jsonNode["data"][0]["pageCount"];
                if (!int.TryParse(jsonNode.ToString(), out var totalPage))
                {
                    return;
                }
                for (int pageNum = 1; pageNum <= totalPage; pageNum++)
                {
                    jsonNode = JsonNode.Parse(await (await client.GetAsync($"front/goods/api/goodslist?keyword=&storeId=&brandId=&gcId=&goodsSpec=&arrivalCycle=&sortField=&sortOrder=&dosageForm=&maximumPrice=&minimumPrice=&pageSize=12&pageNo={pageNum}&activityId=&prdId=")).Content.ReadAsStringAsync());
                    JsonArray jsonArray = (JsonArray)jsonNode["data"][0]["listApiGoods"];
                    List<Product> productList = new(jsonArray.Count);
                    await Parallel.ForEachAsync(jsonArray, async (node, CancellationToken) =>
                    {
                        string Id = node["goodsId"].ToString();
                        JsonNode infoJsonNode = JsonNode.Parse(await client.GetStringAsync("front/goods/api/getGoodsById?goodsId=" + Id, CancellationToken))!["data"]![0]!;
                        Product product = new()
                        {
                            Id = "JXYSQS" + AY.Id + Id,
                            Approval = infoJsonNode["approvalNumber"].ToString(),
                            Name = infoJsonNode["goodsName"].ToString(),
                            Specs = infoJsonNode["goodsSpec"].ToString(),
                            Unit = infoJsonNode["goodsUnitName"].ToString(),
                            ProducerName = infoJsonNode["brandName"].ToString(),
                            Price = (decimal)infoJsonNode["taxPrice"],
                            StockAmount = (int)infoJsonNode["goodsTotalStorage"],
                            BigPack = (int)infoJsonNode["packBig"],
                            MidPack = (int)infoJsonNode["packNum"]!,
                            Url = "https://ec.ayyywl.com/goodsDetail/" + Id,
                            PlatformId = AY.Id,
                            SellTip = string.Empty,
                            SaleTip = string.Empty
                        };
                        if (infoJsonNode["endDateStr"] != null && DateTime.TryParseExact(infoJsonNode["endDateStr"].ToString().Remove(10, 11), "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out var expiry))
                        {
                            product.Expiry = expiry;
                        }
                        else
                        {
                            product.Expiry = null;
                        }
                        productList.Add(product);
                    });
                    foreach (Product item in productList)
                    {
                        if (_context.Products.Contains(item))
                        {
                            _context.Products.Update(item);
                        }
                        else
                        {
                            _context.Products.Add(item);
                        }
                    }
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}