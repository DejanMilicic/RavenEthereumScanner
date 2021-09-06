using HashidsNet;
using Nethereum.ABI;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Raven.Client.Documents;
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
using Transaction = Nethereum.RPC.Eth.DTOs.Transaction;

namespace Importer
{
    public class TransactionProcessor
    {
        public const decimal WhaleTransactionSize = 1000;

        private readonly Web3 _client;
        private readonly DocumentStore _store;

        public TransactionProcessor(Web3 client, DocumentStore store)
        {
            _client = client;
            _store = store;
        }

        public async Task ProcessFrom(long blockNumber)
        {
            var networkId = await _client.Net.Version.SendRequestAsync();
            if (networkId == "1")
                Console.WriteLine("Processing out of the mainnet");
            else
                Console.WriteLine("Processing out of other network");

            int retries = 0;
            while (true)
            {
                var block = await GetBlockWithTransactionHashesAsync(blockNumber);
                if (block == null & retries > 3)
                    throw new Exception($"Failure to process block: {blockNumber}");

                if (block == null)
                {
                    // Retry.
                    retries++;

                    Console.WriteLine($"Failure to read block {blockNumber}. Retry number {retries}");
                    Thread.Sleep(10000); // Sleep for 5 seconds.                     
                    continue;
                }

                if (blockNumber % 10 == 0)
                    Console.WriteLine($"Processing block {blockNumber}");

                blockNumber++;


                var transactions = await GetTransactionsByHashes(block);
                if (transactions.Count == 0)
                    continue;

                var session = _store.OpenSession();
                foreach (var transaction in transactions)
                {
                    var transactionInfo = new TransactionInfo
                    {
                        Id = "transaction/" + transaction.TransactionHash,
                        BlockNumber = transaction.BlockNumber.ToUlong(),
                        From = transaction.From,
                        To = transaction.To,
                        Ether = UnitConversion.Convert.FromWei(transaction.Value.Value, UnitConversion.EthUnit.Ether),
                    };

                    session.Store(transactionInfo, transactionInfo.Id);
                }
                session.SaveChanges();      
                

            }
        }

        private string CreateHashAddress(string address)
        {
            var hashids = new Hashids("ethereum-sample");
            var fromArray = HexUTF8String.CreateFromHex(address).ToHexByteArray().AsSpan();
            var intFromArray = MemoryMarshal.Cast<byte, int>(fromArray);
            return hashids.Encode(intFromArray[0], intFromArray[1]) + hashids.Encode(intFromArray[2], intFromArray[3]) + hashids.Encode(intFromArray[4]);
        }

        private async Task<List<Transaction>> GetTransactionsByHashes(BlockWithTransactionHashes block)
        {
            var list = new List<Transaction>();
            foreach( var hash in block.TransactionHashes )
            {
                var transactionSource = await _client.Eth.Transactions.GetTransactionByHash.SendRequestAsync(hash);
                var transactionReceipt = await _client.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(hash).ConfigureAwait(false);
                if (transactionReceipt.Succeeded())
                {
                    
                    var transactionValue = UnitConversion.Convert.FromWei(transactionSource.Value.Value, UnitConversion.EthUnit.Ether);
                    if (transactionValue > WhaleTransactionSize)
                    {
                        list.Add(transactionSource);

                        Console.WriteLine($"Whale found: {transactionValue} - {transactionSource.TransactionHash}");                        
                    }
                }                                    
            }
            return list;
        }

        protected async Task<BlockWithTransactionHashes> GetBlockWithTransactionHashesAsync(long blockNumber)
        {
            try
            {
                var block = await _client.Eth.Blocks.GetBlockWithTransactionsHashesByNumber
                                         .SendRequestAsync(new HexBigInteger(blockNumber))
                                         .ConfigureAwait(false);
                return block;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Block-{blockNumber} Read Error => " + e.InnerException);
                return null;
            }
        }
    }
}
