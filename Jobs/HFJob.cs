using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Quartz;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WorkerService1.Contexts;
using WorkerService1.Models;
using WorkerService1.Utils;

namespace WorkerService1.Jobs
{
    [DisallowConcurrentExecution]
    public partial class HFJob : IJob
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IDbContextFactory<ERPContext> _ERPContextFactory;

        public HFJob(IHttpClientFactory httpClientFactory, IDbContextFactory<ERPContext> ERPContextFactory)
        {
            _httpClientFactory = httpClientFactory;
            _ERPContextFactory = ERPContextFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using ERPContext _context = await _ERPContextFactory.CreateDbContextAsync();
            Models.Platform HF = new()
            {
                Id = 5,
                Name = "洪福"
            };
            if (!_context.Platforms.Contains(HF))
            {
                _context.Platforms.Add(HF);
                _context.SaveChanges();
            }
            EdgeOptions edgeOptions = new() { PageLoadStrategy = PageLoadStrategy.Normal };
            edgeOptions.AddArgument("--headless=new");
            using EdgeDriver edgeDriver = new(edgeOptions);
            edgeDriver.Navigate().GoToUrl("http://ds.lyhfyy.com/login.html");
            IWebElement username = edgeDriver.FindElement(By.XPath("//*[@id=\"user\"]"));
            IWebElement password = edgeDriver.FindElement(By.XPath("//*[@id=\"pass\"]"));
            username.SendKeys("h02550059");
            password.SendKeys("h02550059");
            edgeDriver.FindElement(By.XPath("//*[@id=\"policyChk\"]")).Click();
            IWebElement loginButton = edgeDriver.FindElement(By.XPath("//*[@id=\"accountBtn\"]"));
            loginButton.Click();
            Thread.Sleep(1000);
            edgeDriver.Navigate().GoToUrl("http://ds.lyhfyy.com/xt_allSearch.html?word=&domain=search#1&1&0&0&0&0&0&0&0&0&0");
            Thread.Sleep(2000);
            edgeDriver.FindElement(By.XPath("//*[@id=\"chose_yh\"]")).Click();
            Thread.Sleep(2000);
            var cookieString = new StringBuilder();
            foreach (var cookie in edgeDriver.Manage().Cookies.AllCookies)
            {
                cookieString.Append($"{cookie.Name}={cookie.Value};");
            }
            using var client = _httpClientFactory.CreateClient("HF");
            client.DefaultRequestHeaders.Add("Cookie", cookieString.ToString());
            string totalPageText = MyRegex().Replace(edgeDriver.FindElement(By.XPath("//*[@id=\"DivPages\"]/span[14]")).Text, "");
            int totalPage = totalPageText == null || !int.TryParse(totalPageText, out int _totalPage) ? 200 : _totalPage;
            try
            {
                for (int pageNum = 1; pageNum <= totalPage; pageNum++)
                {
                    ReadOnlyCollection<IWebElement> linkElements = edgeDriver.FindElements(By.XPath("//*[@id=\"allsea_comm\"]/li"));
                    List<string> links = new();
                    foreach (IWebElement element in linkElements)
                    {
                        string link2 = element.FindElement(By.XPath("div[1]/a")).GetAttribute("href");
                        Regex regex = AYRegex();
                        if (regex.IsMatch(link2))
                        {
                            links.Add(link2);
                        }
                    }
                    List<Product> products = new(links.Count);
                    await Parallel.ForEachAsync(links, async (link, CancellationToken) =>
                    {
                        HtmlDocument document = new();
                        document.LoadHtml(await client.GetStringAsync(link, CancellationToken));
                        Product product2 = new()
                        {
                            Name = document.DocumentNode.SelectSingleNode("//*[@id=\"spname\"]").InnerText,
                            Unit = document.DocumentNode.SelectSingleNode("//*[@id=\"jldw\"]").InnerText,
                            Id = "JXYSQS" + HF.Id + document.DocumentNode.SelectSingleNode("//*[@id=\"scbh\"]").InnerText,
                            ProducerName = document.DocumentNode.SelectSingleNode("//*[@id=\"schcj\"]").InnerText,
                            Approval = document.DocumentNode.SelectSingleNode("//*[@id=\"pzwh\"]").InnerText,
                            Specs = document.DocumentNode.SelectSingleNode("//*[@id=\"gg\"]").InnerText,
                            Url = link.ToString(),
                            Message = string.Empty,
                            MidPack = 1,
                            PlatformId = HF.Id,
                            ProductDate = null,
                            IsBanned = false
                        };
                        string priceText = AYRegex1().Replace(document.DocumentNode.SelectSingleNode("//*[@id=\"jg\"]/div[1]/div/span[2]").InnerText, "");
                        if (priceText != null && decimal.TryParse(priceText, out var _price))
                        {
                            product2.Price = _price;
                        }
                        string stockText = AYRegex2().Replace(document.DocumentNode.SelectSingleNode("//*[@id=\"kc\"]").InnerText, "");
                        if (stockText != null && int.TryParse(stockText, out var _stockAmount))
                        {
                            product2.StockAmount = _stockAmount;
                        }
                        string expiryText = document.DocumentNode.SelectSingleNode("//*[@id=\"yxqz\"]").InnerText;
                        if (expiryText != null && DateOnly.TryParseExact(expiryText, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out var _expiry))
                        {
                            product2.Expiry = _expiry;
                        }
                        var bigPackNode = document.DocumentNode.SelectSingleNode("/html/body/div[4]/div[2]/div[1]/div/div[2]/div[3]/div[1]/div[4]/span[2]");
                        string bigPackText = bigPackNode == null ? string.Empty : bigPackNode.InnerText;
                        if (bigPackText != null && int.TryParse(bigPackText, out var _bigPack))
                        {
                            product2.BigPack = _bigPack;
                        }
                        products.Add(product2);
                    });

                    foreach (var product in products)
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
                    edgeDriver.FindElement(By.XPath("//*[@id=\"next\"]")).Click();
                    Thread.Sleep(1000);
                }
            }
            finally
            {
                edgeDriver.Quit();
            }
        }

        [GeneratedRegex("http://*")]
        private static partial Regex AYRegex();
        [GeneratedRegex("[^\\d.\\d]")]
        private static partial Regex AYRegex1();
        [GeneratedRegex("[^0-9]+")]
        private static partial Regex AYRegex2();
        [GeneratedRegex("[^0-9]+")]
        private static partial Regex MyRegex();
    }
}
