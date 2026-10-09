using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDA.Events
{
    public class MoneyWithdrawnEvent : IEvent
    {
        public string AccountId { get; set; }
        public double Amount { get; set; }
        public DateTime Timestamp { get; }

        public MoneyWithdrawnEvent(string accountId, double amount)
        {
            AccountId = accountId;
            Amount = amount;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class AccountWithdrawnEvent : MoneyWithdrawnEvent
    {
        public AccountWithdrawnEvent(string accountId, double amount) : base(accountId, amount)
        {
        }
    }
}

