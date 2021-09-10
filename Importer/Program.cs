using Nethereum.Geth;
using Nethereum.Geth.RPC.GethEth;
using Nethereum.Web3;
using Raven.Client.Documents;
using System;
using System.Threading.Tasks;

namespace Importer
{
    class Program
    {
        static void Main(string[] args)
        {
            Demo().Wait();
        }
        static async Task Demo()
        {
            try
            {
                var store = new DocumentStore
                {
                    Urls = new string[] { "http://localhost:8080" },
                    Database = "ethereum-sample"
                };               
                store.Initialize();

                //var client = new Web3("https://rinkeby.infura.io/v3/7238211010344719ad14a89db874158c");

                long startingBlock = 13126815;
                using (var session = store.OpenSession())
                {
                    var lastProcessed = session.Load<TransactionProcessedMarker>("transaction/whale/lastprocessed");
                    if (lastProcessed != null)
                        startingBlock = lastProcessed.Block + 1;
                }

                var client = new Web3();
                var transactionProcessor = new TransactionProcessor(client, store);
                await transactionProcessor.ProcessEvents(startingBlock);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}

