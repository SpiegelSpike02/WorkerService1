using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkerService1.Contexts;
using WorkerService1.Models;

namespace WorkerService1.Jobs
{
    [DisallowConcurrentExecution]
    public class HHJob : IJob
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

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
