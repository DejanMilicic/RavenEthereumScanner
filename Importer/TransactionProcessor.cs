using HashidsNet;
using Nethereum.ABI;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Nethereum.BlockchainProcessing.BlockStorage.Repositories;
using Nethereum.BlockchainProcessing.Processor;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Transaction = Nethereum.RPC.Eth.DTOs.Transaction;

namespace Importer
{
    public class TransactionProcessor
    {
        public static readonly BigInteger WhaleTransactionSize = UnitConversion.Convert.ToWei(1000, UnitConversion.EthUnit.Ether);

        private readonly Web3 _client;
        private readonly DocumentStore _store;

        public TransactionProcessor(Web3 client, DocumentStore store)
        {
            _client = client;
            _store = store;
        }

        public async Task ProcessEvents(long blockNumber)
        {
            var networkId = await _client.Net.Version.SendRequestAsync();
            if (networkId == "1")
                Console.WriteLine("Processing out of the mainnet");
            else
                Console.WriteLine("Processing out of other network");

            BigInteger headBlockNumber = (await _client.Eth.Blocks.GetBlockNumber.SendRequestAsync()).Value;

            var transactions = new List<TransactionReceiptVO>();
            var filterLogs = new List<FilterLogVO>();

            long InsertTransaction(DocumentStore store, TransactionReceiptVO tx)
            {
                var transaction = tx.Transaction;
                var blockNumber = tx.BlockNumber.ToUlong();

                using var session = _store.OpenSession();

                var transactionValue = UnitConversion.Convert.FromWei(transaction.Value.Value, UnitConversion.EthUnit.Ether);
                var transactionInfo = new TransactionInfo
                {
                    Id = "transaction/" + tx.BlockNumber + "/" + tx.TransactionHash,
                    BlockNumber = blockNumber,
                    From = transaction.From,
                    To = transaction.To,
                    Ether = transactionValue,
                    Timestamp = (long)tx.BlockTimestamp.ToUlong()
                };                
                Console.WriteLine($"Whale found: {tx.BlockNumber} - {transactionValue} - {tx.TransactionHash}");

                session.Store(transactionInfo, transactionInfo.Id);
                session.Store(new TransactionProcessedMarker() { Block = (long)blockNumber }, "transaction/whale/lastprocessed");
                session.SaveChanges();

                return (long)blockNumber + 1;
            }

            var lastBlockNumber = blockNumber;
            while (blockNumber < headBlockNumber)
            {
                try
                {
                    // create our processor
                    var processor = _client.Processing.Blocks.CreateBlockProcessor(steps =>
                    {
                        steps.TransactionStep.SetMatchCriteria(t => t.Transaction.Value.Value > WhaleTransactionSize);
                        steps.TransactionReceiptStep.AddSynchronousProcessorHandler(tx => blockNumber = InsertTransaction(_store, tx));
                    });

                    //if we need to stop the processor mid execution - call cancel on the token
                    var cancellationToken = new CancellationToken();

                    //crawl the blocks
                    await processor.ExecuteAsync(
                        toBlockNumber: headBlockNumber,
                        cancellationToken: cancellationToken,
                        startAtBlockNumberIfNotProcessed: new BigInteger(blockNumber));

                    if (lastBlockNumber == blockNumber)
                        blockNumber++;                    

                    lastBlockNumber = blockNumber;
                }
                catch( Exception ex)
                {
                    Console.WriteLine($"Failure to process block {blockNumber}");
                    blockNumber++;
                }
            }            
        }
    }
}
