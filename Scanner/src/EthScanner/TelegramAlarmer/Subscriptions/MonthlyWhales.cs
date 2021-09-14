using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Exceptions.Documents.Subscriptions;
using TelegramAlarmer.Infrastructure;
using TelegramAlarmer.Models;

namespace TelegramAlarmer.Subscriptions
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
