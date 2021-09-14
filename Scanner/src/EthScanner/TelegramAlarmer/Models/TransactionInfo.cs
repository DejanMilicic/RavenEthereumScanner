namespace TelegramAlarmer.Models
{
    public class TransactionInfo
    {
        public string Id { get; internal set; }
        public ulong BlockNumber { get; internal set; }
        public string From { get; internal set; }
        public string To { get; internal set; }
        public decimal Ether { get; internal set; }
        public int Timestamp { get; internal set; }
    }
}
