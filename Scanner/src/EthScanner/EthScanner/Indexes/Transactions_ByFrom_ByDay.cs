using System;
using System.Linq;
using EthScanner.Models;
using Raven.Client.Documents.Indexes;

namespace EthScanner.Indexes
{
    public class Transactions_ByFrom_ByDay : AbstractIndexCreationTask<TransactionInfo, Transactions_ByFrom_ByDay.Entry>
    {
        public class Entry
        {
            public string From { get; set; }

            public int Transactions { get; set; }

            public decimal Ether { get; set; }

            public DateTime Date { get; set; }
        }

        public Transactions_ByFrom_ByDay()
        {
            Map = transactions => from trx in transactions
                let ts = DateTimeOffset.FromUnixTimeSeconds(trx.Timestamp).UtcDateTime 
                select new Entry
                {
                    From = trx.From,
                    Transactions = 1,
                    Ether = trx.Ether,
                    Date = new DateTime(ts.Year, ts.Month, ts.Day, 0, 0, 0)
                };

            Reduce = results => from result in results
                group result by new {result.From, result.Date }
                into g
                select new Entry
                {
                    From = g.Key.From,
                    Transactions = g.Sum(x => x.Transactions),
                    Ether = g.Sum(x => x.Ether),
                    Date = g.Key.Date
                };

            OutputReduceToCollection = "TransactionsByFromByDays";
        }
    }
}
