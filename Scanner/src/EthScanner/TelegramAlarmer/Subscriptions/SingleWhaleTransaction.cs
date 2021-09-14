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

        public async Task Consume()
        {
            try
            {
                await _store.Subscriptions.GetSubscriptionStateAsync(_subscriptionName);
            }
            catch (SubscriptionDoesNotExistException)
            {
                await _store.Subscriptions.CreateAsync(new SubscriptionCreationOptions<TransactionInfo>
                {
                    Name = _subscriptionName,
                    Filter = trx => trx.Ether > 2000
                });
            }

            TelegramHelper th = new TelegramHelper();

            var subscription = _store.Subscriptions.GetSubscriptionWorker<TransactionInfo>(
                new SubscriptionWorkerOptions(_subscriptionName)
                {
                    CloseWhenNoDocsLeft = false
                });

            await subscription.Run(async batch =>
            {
                foreach (var item in batch.Items)
                {
                    TransactionInfo trx = item.Result;

                    await th.SendMessage($"Whale transaction \nFrom: {trx.From}\nTo: {trx.To}\nETH {trx.Ether}");
                }
            });
        }
    }
}
