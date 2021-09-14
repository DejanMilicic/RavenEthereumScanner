using System;
using System.Threading.Tasks;
using EthScanner.Subscriptions;
using Northwind;
using TelegramAlarmer.Subscriptions;

namespace TelegramAlarmer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var singleWhale = new SingleWhaleTransaction(DocumentStoreHolder.Store).Consume();
            var dailyWhale = new DailyWhales(DocumentStoreHolder.Store).Consume();
            var monthlyWhale = new MonthlyWhales(DocumentStoreHolder.Store).Consume();


            await Task.WhenAll(singleWhale, dailyWhale, monthlyWhale);

            Console.WriteLine("Hello World!");
        }
    }
}
