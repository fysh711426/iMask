using CsvHelper;
using iMask.Data.Models;
using iMask.EF;
using iMask.EF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iMask.Crawler
{
    public class Worker : IHostedService
    {
        private readonly IHostApplicationLifetime _lifeTime;
        private readonly CoreDbContext _db;
        public Worker(IHostApplicationLifetime lifeTime, CoreDbContext db)
        {
            _lifeTime = lifeTime;
            _db = db;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                Console.WriteLine("Finish!!");
                _lifeTime.StopApplication();
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(
                    "http://data.nhi.gov.tw/Datasets/Download.ashx?rid=A21030000I-D50001-001&l=https://data.nhi.gov.tw/resource/mask/maskdata.csv");

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<CSV>();

                    var tran = await _db.Database.BeginTransactionAsync();

                    var amountDictionary = await _db.Amounts.ToDictionaryAsync(it => it.Code);

                    foreach (var record in records)
                    {
                        //更新資料
                        try
                        {
                            var amount = null as Amount;
                            amountDictionary.TryGetValue(record.醫事機構代碼, out amount);
                            if (amount != null)
                            {
                                amount.DateTime = DateTime.Parse(record.來源資料時間);
                                amount.AdultAmount = int.Parse(record.成人口罩總剩餘數);
                                amount.ChildAmount = int.Parse(record.兒童口罩剩餘數);
                                await _db.SaveChangesAsync();

                                Console.WriteLine($"'{record.醫事機構代碼}' update.");
                            }
                            else
                            {
                                throw new Exception("Not found.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"'{record.醫事機構代碼}' error，msg: {ex.Message}");
                        }
                    }

                    tran.Commit();
                }
            }
        }
    }
}
