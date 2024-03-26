using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Quartz;
using System.Globalization;
using System.Text.Json.Nodes;
using WorkerService1.Contexts;
using WorkerService1.Models;
using WorkerService1.Utils;

namespace WorkerService1.Jobs
{
    [DisallowConcurrentExecution]
    public class YBJob(IHttpClientFactory httpClientFactory, IDbContextFactory<ERPContext> ERPContextFactory) : IJob
    {
        public string? Token { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            EdgeOptions edgeOptions = new();
            edgeOptions.AddArgument("--headless=new");
            using EdgeDriver edgeDriver = new(edgeOptions);
            NetworkManager manager = new(edgeDriver);
            manager.NetworkRequestSent += RequestHandler;
            await manager.StartMonitoring();
            using ERPContext _context = await ERPContextFactory.CreateDbContextAsync();
            Models.Platform YB = new()
            {
                Id = 8,
                Name = "亚邦"
            };
            if (!_context.Platforms.Contains(YB))
            {
                _context.Platforms.Add(YB);
                _context.SaveChanges();
            }
            using HttpClient client = httpClientFactory.CreateClient("YB");
            edgeDriver.Navigate().GoToUrl(client.BaseAddress + "/login");
            var username = edgeDriver.FindElement(By.XPath("/html/body/div/div[2]/div/span/div[2]/div[3]/div[2]/div[1]/input"));
            var password = edgeDriver.FindElement(By.XPath("/html/body/div/div[2]/div/span/div[2]/div[3]/div[2]/div[2]/input"));
            var loginbutton = edgeDriver.FindElement(By.XPath("/html/body/div/div[2]/div/span/div[2]/div[3]/div[3]"));
            username.SendKeys("jycz1005168");
            password.SendKeys("1005168");
            loginbutton.Click();
            Thread.Sleep(1000);
            client.DefaultRequestHeaders.Add("Shopping-Access-Token", Token);
            JsonNode node2 = JsonNode.Parse(await (await client.GetAsync("web/product/getlistE?pageNo=1&pageSize=20&productScreenBO.isInventory=0")).Content.ReadAsStringAsync());
            if (!int.TryParse(node2["result"]["pageData"]["pages"].ToString(), out var totalPage))
            {
                return;
            }
            try
            {
                for (int pageNum = 1; pageNum <= totalPage; pageNum++)
                {
                    node2 = JsonNode.Parse(await (await client.GetAsync($"web/product/getlistE?pageNo={pageNum}&pageSize=20&productScreenBO.isInventory=0")).Content.ReadAsStringAsync())["result"]["pageData"];
                    JsonArray array = (JsonArray)node2["records"];
                    List<Product> productList = new List<Product>(array.Count);
                    await Parallel.ForEachAsync(array, async (node, CancellationToken) =>
                    {
                        JsonNode infoNode = JsonNode.Parse(await client.GetStringAsync($"web/product/getProductDetail?productId={node["productId"]}", CancellationToken))["result"];
                        Product product2 = new()
                        {
                            Id = "JXYSQS" + YB.Id + infoNode["productId"],
                            Approval = infoNode["license"].ToString(),
                            Name = infoNode["name"].ToString(),
                            PlatformId = YB.Id,
                            ProducerName = infoNode["manufacturer"].ToString(),
                            Unit = infoNode["unit"].ToString(),
                            Specs = infoNode["format"].ToString(),
                            Url = client.BaseAddress?.ToString() + $"productDetail?productId={infoNode["productId"]}",
                            Message = string.Empty,
                            IsBanned = false
                        };
                        if (infoNode["showPrice"] != null && decimal.TryParse(infoNode["showPrice"].ToString(), out var _price))
                        {
                            product2.Price = _price;
                        }
                        if (infoNode["bigPkgSize"] != null && int.TryParse(infoNode["bigPkgSize"].ToString(), out var _bigPack))
                        {
                            product2.BigPack = _bigPack;
                        }
                        if (infoNode["mediumPkgSize"] != null && int.TryParse(infoNode["mediumPkgSize"].ToString(), out var _midPack))
                        {
                            product2.MidPack = _midPack;
                        }
                        if (infoNode["inventoryNum"] != null && int.TryParse(infoNode["inventoryNum"].ToString(), out var _stockAmount))
                        {
                            product2.StockAmount = _stockAmount;
                        }
                        if (infoNode["inventoryList"][0]["productionDate"] != null && DateOnly.TryParseExact(infoNode["inventoryList"][0]["productionDate"].ToString(), "yyyy-MM-dd 00:00:00", CultureInfo.CurrentCulture, DateTimeStyles.None, out var _productdate))
                        {
                            product2.ProductDate = _productdate;
                        }
                        if (infoNode["inventoryList"][0]["endDate"] != null && DateOnly.TryParseExact(infoNode["inventoryList"][0]["endDate"].ToString(), "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out var _expiry))
                        {
                            product2.Expiry = _expiry;
                        }
                        productList.Add(product2);
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
            finally
            {
                edgeDriver.Quit();
            }
        }

        private void RequestHandler(object? sender, NetworkRequestSentEventArgs e)
        {
            foreach (var header in e.RequestHeaders)
            {
                if (header.Key.Equals("Shopping-Access-Token"))
                {
                    Token = header.Value;
                }
            }
        }
    }
}
