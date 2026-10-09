using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDA.Events
{
    public class AccountCreatedEvent : IEvent
    {
        public string AccountId { get; set; }
        public string OwnerName { get; set; }
        public double InitialBalance { get; set; }
        public DateTime Timestamp { get; }

        public AccountCreatedEvent(string accountId, string ownerName, double initialBalance)
        {
            AccountId = accountId;
            OwnerName = ownerName;
            InitialBalance = initialBalance;
            Timestamp = DateTime.UtcNow;
        }

        public AccountCreatedEvent(string accountId, double initialBalance)
            : this(accountId, "Chưa rõ", initialBalance)
        {
        }
    }
}

