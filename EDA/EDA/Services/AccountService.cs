using EDA.Events;
using System.Text.Json;

namespace EDA.Services
{
    public class Account
    {
        public string AccountId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public double Balance { get; set; }
    }

    public class AccountService
    {
        private readonly IEventBus _eventBus;
        private readonly string _filePath = "accounts.json";
        private readonly Dictionary<string, Account> _accounts = new();

        public AccountService(IEventBus eventBus)
        {
            _eventBus = eventBus;
            LoadFromFile();
        }

        private void LoadFromFile()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var data = JsonSerializer.Deserialize<Dictionary<string, Account>>(json);
                    if (data != null)
                    {
                        foreach (var kv in data)
                        {
                            _accounts[kv.Key] = kv.Value;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void SaveToFile()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_accounts, options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception)
            {
            }
        }

        public void CreateAccount(string accountId, string ownerName, double initialBalance)
        {
            if (_accounts.ContainsKey(accountId))
            {
                Console.WriteLine($"[Lỗi] Tài khoản {accountId} đã tồn tại!");
                return;
            }

            _accounts[accountId] = new Account
            {
                AccountId = accountId,
                OwnerName = ownerName,
                Balance = initialBalance
            };
            SaveToFile();

            Console.WriteLine($"[Dịch vụ Tài khoản] Đã tạo tài khoản {accountId} cho {ownerName} với số dư ban đầu: {initialBalance:N0} VND");

            _eventBus.Publish(new AccountCreatedEvent(accountId, ownerName, initialBalance));
        }

        public void Deposit(string accountId, double amount)
        {
            if (!_accounts.ContainsKey(accountId))
            {
                Console.WriteLine($"[Lỗi] Không tìm thấy tài khoản {accountId}!");
                return;
            }

            if (amount <= 0)
            {
                Console.WriteLine("[Lỗi] Số tiền nạp phải lớn hơn 0!");
                return;
            }

            _accounts[accountId].Balance += amount;
            SaveToFile();

            Console.WriteLine($"[Dịch vụ Tài khoản] Nạp {amount:N0} VND vào tài khoản {accountId}. Số dư hiện tại: {_accounts[accountId].Balance:N0} VND");

            _eventBus.Publish(new MoneyDepositedEvent(accountId, amount));
        }

        public void Withdraw(string accountId, double amount)
        {
            if (!_accounts.ContainsKey(accountId))
            {
                Console.WriteLine($"[Lỗi] Không tìm thấy tài khoản {accountId}!");
                return;
            }

            if (amount <= 0)
            {
                Console.WriteLine("[Lỗi] Số tiền rút phải lớn hơn 0!");
                return;
            }

            if (_accounts[accountId].Balance < amount)
            {
                Console.WriteLine($"[Lỗi] Tài khoản {accountId} không đủ số dư để rút {amount:N0} VND (Số dư hiện tại: {_accounts[accountId].Balance:N0} VND)!");
                return;
            }

            _accounts[accountId].Balance -= amount;
            SaveToFile();

            Console.WriteLine($"[Dịch vụ Tài khoản] Rút {amount:N0} VND từ tài khoản {accountId}. Số dư hiện tại: {_accounts[accountId].Balance:N0} VND");

            _eventBus.Publish(new MoneyWithdrawnEvent(accountId, amount));
        }

        public void Transfer(string fromAccountId, string toAccountId, double amount)
        {
            if (!_accounts.ContainsKey(fromAccountId))
            {
                Console.WriteLine($"[Lỗi] Không tìm thấy tài khoản nguồn {fromAccountId}!");
                return;
            }

            if (!_accounts.ContainsKey(toAccountId))
            {
                Console.WriteLine($"[Lỗi] Không tìm thấy tài khoản đích {toAccountId}!");
                return;
            }

            if (fromAccountId == toAccountId)
            {
                Console.WriteLine("[Lỗi] Tài khoản nguồn và tài khoản đích không được trùng nhau!");
                return;
            }

            if (amount <= 0)
            {
                Console.WriteLine("[Lỗi] Số tiền chuyển phải lớn hơn 0!");
                return;
            }

            if (_accounts[fromAccountId].Balance < amount)
            {
                Console.WriteLine($"[Lỗi] Tài khoản {fromAccountId} không đủ số dư để chuyển {amount:N0} VND (Số dư hiện tại: {_accounts[fromAccountId].Balance:N0} VND)!");
                return;
            }

            _accounts[fromAccountId].Balance -= amount;
            _accounts[toAccountId].Balance += amount;
            SaveToFile();

            Console.WriteLine($"[Dịch vụ Tài khoản] Chuyển thành công {amount:N0} VND từ tài khoản {fromAccountId} sang tài khoản {toAccountId}.");

            _eventBus.Publish(new TranferMoneyEvent(fromAccountId, toAccountId, amount));
        }

        public double GetBalance(string accountId)
        {
            return _accounts.TryGetValue(accountId, out var account) ? account.Balance : 0;
        }

        public Account? GetAccount(string accountId)
        {
            return _accounts.TryGetValue(accountId, out var account) ? account : null;
        }

        public bool AccountExists(string accountId)
        {
            return _accounts.ContainsKey(accountId);
        }

        public int GetAccountsCount() => _accounts.Count;
    }
}
