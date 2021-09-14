using System;
using System.IO;
using System.Threading.Tasks;
using EthScanner.Subscriptions;
using Microsoft.Extensions.Configuration;
using Northwind;
using TelegramAlarmer.Infrastructure;
using TelegramAlarmer.Subscriptions;
using static Raven.Client.Constants;

namespace TelegramAlarmer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json");

            var config = builder.Build();

            var telegramSettings = config.GetSection("Telegram").Get<AppSettings.TelegramSettings>();

            var helper = new TelegramRateLimiter(telegramSettings.Token);

            var singleWhale = new SingleWhaleTransaction(DocumentStoreHolder.Store).Consume(helper);
            var dailyWhale = new DailyWhales(DocumentStoreHolder.Store).Consume(helper);
            var monthlyWhale = new MonthlyWhales(DocumentStoreHolder.Store).Consume(helper);
            var telegramTask = helper.RunAsync();


            await Task.WhenAll(singleWhale, dailyWhale, monthlyWhale);

            Console.WriteLine("Hello World!");
        }
    }
}
