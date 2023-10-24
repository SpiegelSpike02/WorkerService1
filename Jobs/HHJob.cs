using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Quartz;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using WorkerService1.Contexts;
using WorkerService1.Models;

namespace WorkerService1.Jobs
{
    [DisallowConcurrentExecution]
    public partial class HHJob : IJob
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IDbContextFactory<ERPContext> _ERPContextFactory;

        public HHJob(IHttpClientFactory httpClientFactory, IDbContextFactory<ERPContext> ERPContextFactory)
        {
            _httpClientFactory = httpClientFactory;
            _ERPContextFactory = ERPContextFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using ERPContext _context = await _ERPContextFactory.CreateDbContextAsync();
                Platform HH = new()
                {
                    Id = 4,
                    Name = "淮海"
                };
                if (!await _context.Platforms.ContainsAsync(HH))
                {
                    await _context.Platforms.AddAsync(HH);
                    await _context.SaveChangesAsync();
                }

                using HttpClient client = _httpClientFactory.CreateClient("HH");
                var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                     new ("action", "VSCommon.urlRequest"),
                     new ("url", "/_shop/960/search.shtml?withSkus=true&sv=&sn=1")
                });
                var response = await client.PostAsync("https://hhey.shaphar.com/jsonaction/websiteaction.action", content);
                var _ = JsonNode.Parse(await response.Content.ReadAsStringAsync());
                if (int.TryParse(_["totals"].ToString(), out int _totalNum))
                {
                    Console.WriteLine(_totalNum);
                    var pageNum = 144;
                    while (_context.Products.Where(x => x.PlatformId == 4).Count() < _totalNum)
                    {
                        content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                        {
                            new ("action", "VSCommon.urlRequest"),
                            new ("url", $"/_shop/960/search.shtml?withSkus=true&sv=&sn={pageNum++}")
                        });
                        response = await client.PostAsync("https://hhey.shaphar.com/jsonaction/websiteaction.action", content);
                        var totalNode = JsonNode.Parse(await response.Content.ReadAsStringAsync());
                        var array = (JsonArray)totalNode["results"];
                        List<Product> productList = new(array.Count);
                        await Parallel.ForEachAsync(array, async (node, CancellationToken) =>
                        {
                            var product = new Product()
                            {
                                Id = "JXYSQS" + HH.Id + node["productCodeOfOrg"],
                                Name = node["productName"].ToString(),
                                Specs = node["spec"].ToString(),
                                Unit = node["units"].ToString(),
                                Url = "https://hhey.shaphar.com" + node["url"],
                                ProducerName = node["factoryName"].ToString(),
                                SellTip = string.Empty,
                                SaleTip = string.Empty,
                                PlatformId = HH.Id
                            };
                            JToken skus = JObject.Parse(node["skus"].ToString()).First.Children().First();
                            if (decimal.TryParse(skus["price"].ToString(), out decimal _price))
                            {
                                product.Price = _price;
                            }
                            if (int.TryParse(skus["bigPackAmount"].ToString(), out int _bigPack))
                            {
                                product.BigPack = _bigPack;
                            }
                            if (int.TryParse(skus["packAmount"].ToString(), out int _midPack))
                            {
                                product.MidPack = _midPack;
                            }
                            if (int.TryParse(skus["stockTag"]["amount"].ToString(), out int _stockAmount))
                            {
                                product.StockAmount = _stockAmount;
                            }
                            MatchCollection mt = MyRegex().Matches(skus["stockTag"]["stockState"].ToString());
                            if (mt.Count > 0 && DateTime.TryParseExact(mt.First().ToString(), "yyyy/MM/dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out var _expiry))
                            {
                                product.Expiry = _expiry;
                            }
                            HtmlDocument htmlDocument = new();
                            htmlDocument.LoadHtml(await client.GetStringAsync(product.Url, CancellationToken));
                            var approvalElements = htmlDocument.DocumentNode.Descendants(0).Where(n => n.HasClass("approvalNO"));
                            if (approvalElements.Any())
                            {
                                var approval = approvalElements.First();
                                if (approval != null && approval.GetDirectInnerText != null)
                                {
                                    product.Approval = approval.GetDirectInnerText().Trim();
                                }
                                else
                                {
                                    product.Approval = "无";
                                }
                            }
                            else
                            {
                                product.Approval = "无";
                            }
                            productList.Add(product);
                        });
                        foreach (var product in productList)
                        {
                            if (_context.Products.AsNoTracking().Any(x => x.Id == product.Id))
                            {
                                var _product = _context.Products.AsNoTracking().First(x => x.Id == product.Id);
                                _product.Expiry = product.Expiry;
                                _product.Price = product.Price;
                                _product.Approval = product.Approval;
                                _product.ProductDate = product.ProductDate;
                                _product.StockAmount = product.StockAmount;
                                _product.Url = _product.Url;
                            }
                            else
                            {
                                _context.Products.Add(product);
                            }
                        }
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [GeneratedRegex("([0-9]{4}/[0-9]{2}/[0-9]{2})")]
        private static partial Regex MyRegex();
    }
}
