using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Exceptions.Documents.Subscriptions;
using TelegramAlarmer.Infrastructure;
using TelegramAlarmer.Models;

namespace TelegramAlarmer.Subscriptions
{
    public class SingleWhaleTransaction
    {
        private readonly IDocumentStore _store;
        private readonly string _subscriptionName = "SingleWhaleTransactions";

        public SingleWhaleTransaction(IDocumentStore store)
        {
            _store = store;
        }

        public async Task Consume(TelegramRateLimiter th)
        {
            var subscription = _store.Subscriptions.GetSubscriptionWorker<TransactionInfo>(
                new SubscriptionWorkerOptions(_subscriptionName)
                {
                    CloseWhenNoDocsLeft = false
                });

            await subscription.Run(batch =>
            {
                foreach (var item in batch.Items)
                {
                    TransactionInfo trx = item.Result;

                    th.SendMessage($"Whale transaction \nFrom: {trx.From}\nTo: {trx.To}\nETH {trx.Ether}");                    
                }
            });
        }
    }    
}
