using System;
using System.Threading.Tasks;
using EthScanner.Features;
using EthScanner.Infrastructure;
using EthScanner.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Exceptions.Documents.Subscriptions;

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

        public async Task Create(TelegramRateLimiter th)
        {
            try
            {
                await _store.Subscriptions.GetSubscriptionStateAsync(_subscriptionName);
            }
            catch (SubscriptionDoesNotExistException)
            {
                await _store.Subscriptions.CreateAsync(new SubscriptionCreationOptions<TransactionsByFromByDay>
                {
                    Name = _subscriptionName,
                    Filter = trx => trx.Ether > 10000 || trx.Transactions > 2,
                    ChangeVector = "LastDocument"
                });
            }

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
