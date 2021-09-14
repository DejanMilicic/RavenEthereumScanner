using System;
using System.Threading.Tasks;
using EthScanner.Infrastructure;
using EthScanner.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Exceptions.Documents.Subscriptions;

namespace EthScanner.Subscriptions
{
    public class MonthlyWhales
    {
        private readonly IDocumentStore _store;
        private readonly string _subscriptionName = "MonthlyWhales";

        public MonthlyWhales(IDocumentStore store)
        {
            _store = store;
        }

        public async Task Consume()
        {
            try
            {
                await _store.Subscriptions.GetSubscriptionStateAsync(_subscriptionName);
            }
            catch (SubscriptionDoesNotExistException)
            {
                await _store.Subscriptions.CreateAsync(new SubscriptionCreationOptions<TransactionsByFromByMonth>
                {
                    Name = _subscriptionName,
                    Filter = trx => trx.Ether > 100000 || trx.Transactions > 20
                });
            }

            TelegramHelper th = new TelegramHelper();


            var subscription = _store.Subscriptions.GetSubscriptionWorker<TransactionsByFromByMonth>(
                new SubscriptionWorkerOptions(_subscriptionName)
                {
                    CloseWhenNoDocsLeft = false
                });
            await subscription.Run(async batch =>
            {
                foreach (var item in batch.Items)
                {
                    TransactionsByFromByMonth trx = item.Result;

                    await th.SendMessage(
                        $"Monthly Whale \nFrom: {trx.From}\nTransactions: {trx.Transactions}\nETH {trx.Ether}");
                }
            });
        }
    }
}
