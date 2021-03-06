using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Exceptions.Documents.Subscriptions;
using TelegramAlarmer.Infrastructure;
using TelegramAlarmer.Models;
using TelegramAlarmer.Subscriptions;

namespace EthScanner.Subscriptions
{
    public class DailyWhales
    {
        private readonly IDocumentStore _store;
        private readonly string _subscriptionName = "DailyWhales";

        public DailyWhales(IDocumentStore store)
        {
            _store = store;
        }

        public async Task Consume(TelegramRateLimiter th)
        {
            var subscription = _store.Subscriptions.GetSubscriptionWorker<TransactionsByFromByDay>(
                new SubscriptionWorkerOptions(_subscriptionName)
                {
                    CloseWhenNoDocsLeft = false
                });
            await subscription.Run(batch =>
            {
                foreach (var item in batch.Items)
                {
                    TransactionsByFromByDay trx = item.Result;
                    th.SendMessage($"Daily Whale \nFrom: {trx.From}\nTransactions: {trx.Transactions}\nETH {trx.Ether}");
                }
            });
        }
    }
}
