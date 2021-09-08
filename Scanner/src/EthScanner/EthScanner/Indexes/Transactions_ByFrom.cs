using System.Linq;
using EthScanner.Models;
using Raven.Client.Documents.Indexes;

namespace EthScanner.Indexes
{
    public class Transactions_ByFrom : AbstractIndexCreationTask<TransactionInfo, Transactions_ByFrom.Entry>
    {
        public class Entry
        {
            public string From { get; set; }

            public int Transactions { get; set; }

            public decimal Ether { get; set; }
        }

        public Transactions_ByFrom()
        {
            Map = transactions => from trx in transactions
                select new Entry
                {
                    From = trx.From,
                    Transactions = 1,
                    Ether = trx.Ether
                };

            Reduce = results => from result in results
                group result by result.From
                into g
                select new Entry
                {
                    From = g.Key,
                    Transactions = g.Sum(x => x.Transactions),
                    Ether = g.Sum(x => x.Ether)
                };
        }
    }
}
