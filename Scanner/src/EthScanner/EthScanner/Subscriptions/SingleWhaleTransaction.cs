using System.Threading.Tasks;
using EthScanner.Infrastructure;
using EthScanner.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Exceptions.Documents.Subscriptions;

namespace EthScanner.Subscriptions
{
    public class SingleWhaleTransaction
    {
        private readonly IDocumentStore _store;

        public SingleWhaleTransaction(IDocumentStore store)
        {
            _store = store;
        }

        public async Task Consume()
        {
            try
            {
                await _store.Subscriptions.GetSubscriptionStateAsync("SingleWhaleTransactions");
            }
            catch (SubscriptionDoesNotExistException)
            {
                await _store.Subscriptions.CreateAsync(new SubscriptionCreationOptions<TransactionInfo>
                {
                    Name = "SingleWhaleTransactions",
                    Filter = trx => trx.Ether > Constants.WhaleEth
                });
            }

            TelegramHelper th = new TelegramHelper();

            var subscription = _store.Subscriptions.GetSubscriptionWorker<TransactionInfo>(
                new SubscriptionWorkerOptions("SingleWhaleTransactions"));
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
