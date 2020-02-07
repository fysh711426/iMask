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
                    "https://data.nhi.gov.tw/Datasets/DatasetResource.ashx?rId=A21030000I-D21005-004");

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<CSV>();

                    var isFirst = true;
                    foreach (var record in records)
                    {
                        //判斷資料需不需要更新
                        if (isFirst)
                        {
                            var amount = await _db.Amounts.Where(it => it.IsEnable == 1)
                                .FirstOrDefaultAsync();
                            if (amount?.DateTime.ToString("yyyy/MM/dd HH:mm") == record.來源資料時間)
                                break;
                            isFirst = false;
                        }
                        //將資料都設定不啟用
                        var amountList = await _db.Amounts.Where(it => it.IsEnable == 1)
                            .ToListAsync();
                        foreach(var item in amountList)
                        {
                            item.IsEnable = 0;
                        }
                        await _db.SaveChangesAsync();

                        //更新資料
                        var shopDictionary = await _db.Shops
                            .ToDictionaryAsync(it => it.Code);
                        var shop = null as Shop;
                        if (shopDictionary.TryGetValue(record.醫事機構代碼, out shop))
                        {
                            var amount = new Amount
                            {
                                ShopId = shop.Id,
                                DateTime = DateTime.Parse(record.來源資料時間),
                                IsEnable = 1,
                                AdultAmount = int.Parse(record.成人口罩總剩餘數),
                                ChildAmount = int.Parse(record.兒童口罩剩餘數)
                            };
                            _db.Amounts.Add(amount);
                        }
                        else
                        {
                            Console.WriteLine($"查無 {record.醫事機構代碼} 代碼。");
                        }
                        await _db.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
