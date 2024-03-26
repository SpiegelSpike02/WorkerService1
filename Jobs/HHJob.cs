using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Quartz;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using WorkerService1.Contexts;
using WorkerService1.Models;
using WorkerService1.Utils;

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
            using ERPContext _context = await _ERPContextFactory.CreateDbContextAsync();
            Models.Platform HH = new()
            {
                Id = 1,
                Name = "淮海"
            };
            if (!_context.Platforms.Contains(HH))
            {
                _context.Platforms.Add(HH);
                _context.SaveChanges();
            }
            using HttpClient client = _httpClientFactory.CreateClient("HH");
            StringBuilder cookieString = new();
            EdgeOptions edgeOptions = new() { PageLoadStrategy = PageLoadStrategy.Normal };
            edgeOptions.AddArgument("--headless=new");
            using EdgeDriver driver = new(edgeOptions);
            driver.Navigate().GoToUrl("https://hhey.shaphar.com/");
            var userName = driver.FindElement(By.XPath("//*[@id=\"userName\"]"));
            userName.SendKeys("xzhx");
            var password = driver.FindElement(By.XPath("//*[@id=\"password\"]"));
            password.SendKeys("123456");
            var logInButton = driver.FindElement(By.XPath("//*[@id=\"btn_sub\"]"));
            logInButton.Click();
            Thread.Sleep(1000);
            foreach (var cookie in driver.Manage().Cookies.AllCookies)
            {
                cookieString.Append($"{cookie.Name}={cookie.Value};");
            }
            driver.Quit();
            client.DefaultRequestHeaders.Add("Cookie", cookieString.ToString());
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
                var pageNum = 1;
                while (pageNum <= _totalNum / 40)
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
                            Id = "JXYSQH1" + node["productCodeOfOrg"],
                            Name = node["productName"].ToString(),
                            Specs = node["spec"].ToString(),
                            Unit = node["units"].ToString(),
                            Url = "https://hhey.shaphar.com" + node["url"],
                            ProducerName = node["factoryName"].ToString(),
                            Message = string.Empty,
                            PlatformId = HH.Id,
                            Approval = string.Empty,
                            IsBanned = false
                        };
                        JToken skus = JObject.Parse(node["skus"].ToString()).First.Children().First();
                        if (!(bool)skus["canSell"])
                        {
                            product.IsBanned = true;
                        }
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
                        if (mt.Count > 0 && DateOnly.TryParseExact(mt.First().ToString(), "yyyy/MM/dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out var _expiry))
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
                        }
                        productList.Add(product);
                    });
                    foreach (var product in productList)
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

                        hhyy hhyy = DataConverter.Convert(product);
                        if (_context.hhyys.AsNoTracking().Any(x => x.skuCode.Equals(hhyy.skuCode)))
                        {
                            var _hhyy = _context.hhyys.First(x => x.skuCode.Equals(hhyy.skuCode));
                            _hhyy.stockState = hhyy.stockState;
                            _hhyy.price = hhyy.price;
                            _hhyy.approvalNO = hhyy.approvalNO;
                            _hhyy.amount = hhyy.amount;
                            _hhyy.url = hhyy.url;
                            _hhyy.noSellTip = hhyy.noSellTip;
                        }
                        else
                        {
                            _context.hhyys.Add(hhyy);
                        }
                    }
                    _context.SaveChanges();
                }
            }
        }

        [GeneratedRegex("([0-9]{4}/[0-9]{2}/[0-9]{2})")]
        private static partial Regex MyRegex();
    }
}
