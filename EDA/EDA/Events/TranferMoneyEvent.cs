namespace EDA.Events
{
    public class TranferMoneyEvent : IEvent
    {
        public string FromAccountId { get; set; }
        public string ToAccountId { get; set; }
        public double Amount { get; set; }
        public DateTime Timestamp { get; }

        public TranferMoneyEvent(string fromAccountId, string toAccountId, double amount)
        {
            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;
            Amount = amount;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class TransferMoneyEvent : TranferMoneyEvent
    {
        public TransferMoneyEvent(string fromAccountId, string toAccountId, double amount)
            : base(fromAccountId, toAccountId, amount)
        {
        }
    }
}
