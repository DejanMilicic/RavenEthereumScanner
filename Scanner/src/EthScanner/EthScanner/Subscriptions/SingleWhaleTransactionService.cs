using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EthScanner.Infrastructure;
using EthScanner.Models;
using Microsoft.Extensions.Hosting;
using Raven.Client.Documents;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Exceptions.Documents.Subscriptions;

namespace EthScanner.Subscriptions
{
    public class SingleWhaleTransactionService : IHostedService
    {
        private readonly IDocumentStore _store;
        private readonly string _subscriptionName = "SingleWhaleTransactions";

        public SingleWhaleTransactionService(IDocumentStore store)
        {
            _store = store;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _store.Subscriptions.GetSubscriptionStateAsync(_subscriptionName, token: cancellationToken);
            }
            catch (SubscriptionDoesNotExistException)
            {
                await _store.Subscriptions.CreateAsync(
                    options: new SubscriptionCreationOptions<TransactionInfo>
                    {
                        Name = _subscriptionName,
                        Filter = trx => trx.Ether > 2000
                    },
                    token: cancellationToken);
            }

            TelegramHelper th = new TelegramHelper();

            SubscriptionWorker<TransactionInfo> subscription = _store.Subscriptions.GetSubscriptionWorker<TransactionInfo>(
                new SubscriptionWorkerOptions(_subscriptionName)
                {
                    CloseWhenNoDocsLeft = false
                });

            await subscription.Run(async batch =>
            {
                foreach (var item in batch.Items)
                {
                    TransactionInfo trx = item.Result;

                    try
                    {
                       await th.SendMessage($"Whale transaction \nFrom: {trx.From}\nTo: {trx.To}\nETH {trx.Ether}");
                    }
                    catch
                    {

                    }
                }
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
