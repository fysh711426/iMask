using CsvHelper;
using iMask.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
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
        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        private static object locker = new object();
        public FeatureCollection GetJson()
        {
            var cacheName = "Json";

            var val = _memoryCache.Get<FeatureCollection>(cacheName);
            if (val == null)
            {
                lock (locker)
                {
                    val = _memoryCache.Get<FeatureCollection>(cacheName);
                    if (val == null)
                    {
                        var json = loadJson().ConfigureAwait(false).GetAwaiter().GetResult();

                        val = json;

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
        
        private async Task<FeatureCollection> loadJson()
        {
            using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetStringAsync(
                    "https://raw.githubusercontent.com/kiang/pharmacies/master/json/points.json");

                return JsonConvert.DeserializeObject<FeatureCollection>(json);
            }
        }
    }
}
