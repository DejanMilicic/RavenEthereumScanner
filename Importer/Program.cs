using Nethereum.Geth;
using Nethereum.Geth.RPC.GethEth;
using Nethereum.Web3;
using Raven.Client.Documents;
using System;
using System.Security.Cryptography.X509Certificates;
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
            string certThumbPrint = "5867BAFD843F9BC761D5790EF1ED13FC454C1A35";

            // Try to open the store.
            using var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);
            // Find the certificate that matches the thumbprint.
            var certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, certThumbPrint, false);
            certStore.Close();

            // Check to see if our certificate was added to the collection. If no, 
            // throw an error, if yes, create a certificate using it.
            if (certCollection.Count == 0)
            {
                Console.WriteLine("Error: No certificate found containing thumbprint ");
                Console.ReadLine();
                Environment.Exit(-1);
            }

            try
            {



                var store = new DocumentStore
                {
                    Urls = new string[] { "https://a.eth-dev.dejan.ravendb.cloud" },
                    Database = "eth",
                    Certificate = certCollection[0]
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

