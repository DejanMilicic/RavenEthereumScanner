using System;
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

            public decimal Ether { get; set; }

            public DateTime Date { get; set; }
        }

        public Transactions_ByFrom()
        {
            Map = transactions => from trx in transactions
                let ts = DateTimeOffset.FromUnixTimeSeconds(trx.Timestamp).UtcDateTime 
                select new Entry
                {
                    From = trx.From,
                    Ether = trx.Ether,
                    Date = new DateTime(ts.Year, ts.Month, 1, 0, 0, 0)
                };
        }
    }
}
