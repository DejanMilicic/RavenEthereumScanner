using System;
using System.Threading.Tasks;
using Northwind;
using TelegramAlarmer.Subscriptions;

namespace TelegramAlarmer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new SingleWhaleTransaction(DocumentStoreHolder.Store).Consume();

            Console.WriteLine("Hello World!");
        }
    }
}
