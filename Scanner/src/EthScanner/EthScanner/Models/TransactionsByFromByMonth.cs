using System;

namespace EthScanner.Models
{
    public class TransactionsByFromByMonth
    {
        public string Id { get; set; }

        public DateTime Date { get; set; }
        
        public float Ether { get; set; }
        
        public string From { get; set; }
        
        public int Transactions { get; set; }
    }
}
