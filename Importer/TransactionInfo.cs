using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importer
{
    public class TransactionInfo
    {
        public string Id { get; internal set; }
        public ulong BlockNumber { get; internal set; }
        public string From { get; internal set; }
        public string To { get; internal set; }
        public decimal Ether { get; internal set; }
    }
}
