using CsvHelper;
using iMask.EF;
using iMask.EF.Models;
using iMask.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace iMask
{
    public class CacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CoreDbContext _db;
        public CacheService(IMemoryCache memoryCache, CoreDbContext db)
        {
            _memoryCache = memoryCache;
            _db = db;
        }

        private static object locker = new object();
        public List<Amount> GetAmountList()
        {
            var cacheName = "AmountList";

            var val = _memoryCache.Get<List<Amount>>(cacheName);
            if (val == null)
            {
                lock (locker)
                {
                    val = _memoryCache.Get<List<Amount>>(cacheName);
                    if (val == null)
                    {
                        var amountList = _db.Amounts.AsNoTracking().ToList();

                        val = amountList;

                        _memoryCache.Set(cacheName, val, new MemoryCacheEntryOptions
                        {
                            //24小時後過期
                            AbsoluteExpiration = DateTimeOffset.Now.AddHours(24)
                        });
                    }
                }
            }
            return val;
        }

        private static object csvLocker = new object();
        public Dictionary<string, CSV> GetCSV()
        {
            var cacheName = "CSV";

            var val = _memoryCache.Get<Dictionary<string, CSV>>(cacheName);
            if (val == null)
            {
                lock (locker)
                {
                    val = _memoryCache.Get<Dictionary<string, CSV>>(cacheName);
                    if (val == null)
                    {
                        var csv = loadCSV().ConfigureAwait(false).GetAwaiter().GetResult();

                        val = csv;

                        _memoryCache.Set(cacheName, val, new MemoryCacheEntryOptions
                        {
                            //30秒後過期
                            AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(30)
                        });
                    }
                }
            }
            return val;
        }

        private async Task<Dictionary<string, CSV>> loadCSV()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(
                    "http://data.nhi.gov.tw/Datasets/Download.ashx?rid=A21030000I-D50001-001&l=https://data.nhi.gov.tw/resource/mask/maskdata.csv");

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    return csv.GetRecords<CSV>().ToDictionary(it => it.醫事機構代碼);
                }
            }
        }
    }
}
