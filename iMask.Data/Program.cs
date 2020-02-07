using iMask.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace iMask.Data
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    //將 appsettings.json 加入 Configuration
                    config.AddJsonFile("appsettings.json", optional: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    //將 CoreDbContext 註冊 DI
                    services.AddDbContext<CoreDbContext>(options =>
                    {
                        options.UseMySQL(hostContext.Configuration.GetConnectionString("SQLConnectionString"));
                    });
                    services.AddHostedService<Worker>();
                });
    }
}
