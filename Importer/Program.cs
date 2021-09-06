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

                var transactionProcessor = new TransactionProcessor(new Web3(), store);
                await transactionProcessor.ProcessFrom(13100000);
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

