using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using WorkerService1.Contexts;
using WorkerService1.Models;

namespace WorkerService1.Jobs
{
    [DisallowConcurrentExecution]
    public class LQJob(IHttpClientFactory httpClientFactory, IDbContextFactory<ERPContext> ERPContextFactory) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using ERPContext _context = await ERPContextFactory.CreateDbContextAsync();
            Platform LQ = new()
            {
                Id = 6,
                Name = "立强"
            };
            if (!_context.Platforms.Contains(LQ))
            {
                _context.Platforms.Add(LQ);
                _context.SaveChanges();
            }
            using HttpClient client = httpClientFactory.CreateClient("LQ");
            FormUrlEncodedContent content = new(new KeyValuePair<string, string>[3]
                {
                new KeyValuePair<string, string>("username", "14639"),
                new KeyValuePair<string, string>("userpass", "123456"),
                new KeyValuePair<string, string>("do", "login")
                });
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            await client.PostAsync("login.html", content);
            HtmlDocument _document = new();
            HtmlDocument htmlDocument = _document;
            htmlDocument.LoadHtml(await client.GetStringAsync("goods.html"));
            int maxPage = ((!int.TryParse(Regex.Replace(_document.DocumentNode.SelectSingleNode("/html/body/div[4]/div/div[4]/span[1]").InnerText, "[^0-9]+", ""), out int _maxpage)) ? 5000 : _maxpage);
            for (int pageNum3 = 1; pageNum3 < maxPage; pageNum3++)
            {
                htmlDocument = new HtmlDocument();
                HtmlDocument htmlDocument2 = htmlDocument;
                htmlDocument2.LoadHtml(await client.GetStringAsync($"goods-all-filter-0,0,0,0,0,0,1,1-{pageNum3}.html"));
                HtmlNodeCollection list = htmlDocument.DocumentNode.SelectNodes("//*[@id=\"pro_list1\"]/li");
                List<Product> productList2 = new();
                await Parallel.ForEachAsync(list, async (node, CancellationToken) =>
                {
                    HtmlDocument innerDoc2 = new();
                    string url2 = node.SelectSingleNode("div[1]/a").GetAttributeValue("href", "").Remove(0, 1);
                    HtmlDocument htmlDocument4 = innerDoc2;
                    htmlDocument4.LoadHtml(await client.GetStringAsync(url2, CancellationToken));
                    HtmlNodeCollection infos2 = innerDoc2.DocumentNode.SelectNodes("/html/body/div[4]/div/div[1]/div[2]/div");
                    Product product5 = new()
                    {
                        Id = "JXYSQS" + LQ.Id + innerDoc2.DocumentNode.SelectSingleNode("/html/body/div[4]/div/div[2]/div[2]/div[2]/ul/li[1]").InnerText.Remove(0, 5),
                        Name = ((innerDoc2.DocumentNode.SelectSingleNode("/html/body/div[4]/div/div[1]/div[2]/h1") != null) ? innerDoc2.DocumentNode.SelectSingleNode("/html/body/div[4]/div/div[1]/div[2]/h1").InnerText : "无"),
                        PlatformId = LQ.Id,
                        Url = client.BaseAddress?.ToString() + url2,
                        Message = string.Empty,
                        Approval = string.Empty,
                        Specs = string.Empty,
                        IsBanned = false,
                        ProducerName = string.Empty
                    };
                    foreach (HtmlNode info2 in infos2)
                    {
                        switch (info2.InnerText)
                        {
                            case "会 员 价：":
                                {
                                    if (decimal.TryParse(Regex.Replace(info2.NextSibling.SelectSingleNode("span").GetDirectInnerText(), "[^0-9,.]+", ""), out var _price2))
                                    {
                                        product5.Price = _price2;
                                    }
                                    else
                                    {
                                        product5.Price = default(decimal);
                                    }
                                    break;
                                }
                            case "库存数量：":
                                {
                                    if (int.TryParse(info2.NextSibling.SelectSingleNode("input").GetAttributeValue("value", ""), out var _stock2))
                                    {
                                        product5.StockAmount = _stock2;
                                    }
                                    else
                                    {
                                        product5.StockAmount = 0;
                                    }
                                    break;
                                }
                            case "批准文号：":
                                product5.Approval = info2.NextSibling.InnerText ?? "无";
                                break;
                            case "件 装 量：":
                                {
                                    if (int.TryParse(info2.NextSibling.InnerText, out var _bigpack2))
                                    {
                                        product5.BigPack = _bigpack2;
                                    }
                                    else
                                    {
                                        product5.BigPack = 1;
                                    }
                                    break;
                                }
                            case "中 包 装：":
                                {
                                    if (int.TryParse(info2.NextSibling.InnerText, out var _midpack2))
                                    {
                                        product5.MidPack = _midpack2;
                                    }
                                    else
                                    {
                                        product5.MidPack = 1;
                                    }
                                    break;
                                }
                            case "生产企业：":
                                product5.ProducerName = info2.NextSibling.InnerText ?? "无";
                                break;
                            case "生产日期：":
                                {
                                    if (DateOnly.TryParseExact(info2.NextSibling.InnerText, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out var _productdate2))
                                    {
                                        product5.ProductDate = _productdate2;
                                    }
                                    else
                                    {
                                        product5.ProductDate = null;
                                    }
                                    break;
                                }
                            case "有 效 期：":
                                {
                                    if (DateOnly.TryParseExact(info2.NextSibling.InnerText, "yyyy年MM月", CultureInfo.CurrentCulture, DateTimeStyles.None, out var _expiry2))
                                    {
                                        product5.Expiry = _expiry2;
                                    }
                                    else
                                    {
                                        product5.Expiry = null;
                                    }
                                    break;
                                }
                            case "商品规格：":
                                product5.Specs = info2.NextSibling.InnerText ?? "无";
                                break;
                            case "优惠信息：":
                                product5.Message = info2.NextSibling.InnerText;
                                break;
                        }
                    }
                    productList2.Add(product5);
                });
                foreach (var product in productList2)
                {
                    if (_context.Products.AsNoTracking().Any(x => x.Id.Equals(product.Id)))
                    {
                        var _product = _context.Products.First(x => x.Id.Equals(product.Id));
                        _product.Expiry = product.Expiry;
                        _product.Price = product.Price;
                        _product.Approval = product.Approval;
                        _product.ProductDate = product.ProductDate;
                        _product.StockAmount = product.StockAmount;
                        _product.Url = _product.Url;
                        _product.Message = product.Message;
                        _product.IsBanned = product.IsBanned;
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
}
