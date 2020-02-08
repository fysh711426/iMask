using CsvHelper;
using CsvHelper.Configuration;
using GoogleMaps.LocationServices;
using iMask.Data.Models;
using iMask.EF;
using iMask.EF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
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

namespace iMask.Data
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
                var json = await httpClient.GetStringAsync(
                    "https://raw.githubusercontent.com/kiang/pharmacies/master/json/points.json?fbclid=IwAR0jokvot6wvsC1w1d42s-rF7TtqNZYB58Bv88pMG8JJ-8jbS751HRQUxZ0");

                var datas = JsonConvert.DeserializeObject<FeatureCollection>(json);

                var tran = await _db.Database.BeginTransactionAsync();

                var amountDictionary = await _db.Amounts.ToDictionaryAsync(it => it.Code);

                foreach (var feature in datas.features)
                {
                    try
                    {
                        var item = feature.properties;
                        var amount = null as Amount;
                        amountDictionary.TryGetValue(item.id, out amount);
                        if (amount == null)
                        {
                            amount = new Amount
                            {
                                Code = item.id,
                                Name = item.name,
                                Address = item.address,
                                Phone = item.phone,
                                Latitude = feature.geometry.coordinates[1],
                                Longitude = feature.geometry.coordinates[0],
                                DateTime = null,
                                AdultAmount = null,
                                ChildAmount = null
                            };
                            _db.Amounts.Add(amount);
                        }
                        else
                        {
                            amount.Name = item.name;
                            amount.Address = item.address;
                            amount.Phone = item.phone;
                            amount.Latitude = feature.geometry.coordinates[1];
                            amount.Longitude = feature.geometry.coordinates[0];
                        }
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"'{feature.properties.id}' error，msg: {ex.Message}");
                    }
                }

                tran.Commit();
            }
        }
    }
}
