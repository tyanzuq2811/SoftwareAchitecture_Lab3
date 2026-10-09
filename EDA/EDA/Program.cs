using EDA;
using EDA.Events;
using EDA.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

IEventBus eventBus = new RabbitMQEventBus();

eventBus.Subscribe<AccountCreatedEvent>(e =>
{
    Console.WriteLine($"\n[Dịch vụ Email] Gửi email chào mừng: Tài khoản '{e.AccountId}' của khách hàng '{e.OwnerName}' đã được kích hoạt thành công!");
});

eventBus.Subscribe<MoneyDepositedEvent>(e =>
{
    Console.WriteLine($"\n[Dịch vụ SMS] Biến động số dư: +{e.Amount:N0} VND vào tài khoản {e.AccountId}. Thời gian: {e.Timestamp:HH:mm:ss}");
});

eventBus.Subscribe<MoneyWithdrawnEvent>(e =>
{
    Console.WriteLine($"\n[Dịch vụ SMS] Biến động số dư: -{e.Amount:N0} VND từ tài khoản {e.AccountId}. Thời gian: {e.Timestamp:HH:mm:ss}");
});

eventBus.Subscribe<TranferMoneyEvent>(e =>
{
    Console.WriteLine($"\n[Dịch vụ SMS] Biến động số dư: -{e.Amount:N0} VND từ tài khoản {e.FromAccountId} (Chuyển tiền đến {e.ToAccountId}). Thời gian: {e.Timestamp:HH:mm:ss}");
    Console.WriteLine($"[Dịch vụ SMS] Biến động số dư: +{e.Amount:N0} VND vào tài khoản {e.ToAccountId} (Nhận tiền từ {e.FromAccountId}). Thời gian: {e.Timestamp:HH:mm:ss}");
});

eventBus.Subscribe<AccountCreatedEvent>(e =>
{
    Console.WriteLine($"[Nhật ký Hệ thống] Ghi nhận: Mở tài khoản {e.AccountId} | Số dư ban đầu: {e.InitialBalance:N0} VND");
});

eventBus.Subscribe<MoneyDepositedEvent>(e =>
{
    Console.WriteLine($"[Nhật ký Hệ thống] Ghi nhận: Nạp tiền vào tài khoản {e.AccountId} | Số tiền: {e.Amount:N0} VND");
});

eventBus.Subscribe<MoneyWithdrawnEvent>(e =>
{
    Console.WriteLine($"[Nhật ký Hệ thống] Ghi nhận: Rút tiền từ tài khoản {e.AccountId} | Số tiền: {e.Amount:N0} VND");
});

eventBus.Subscribe<TranferMoneyEvent>(e =>
{
    Console.WriteLine($"[Nhật ký Hệ thống] Ghi nhận: Chuyển tiền từ tài khoản {e.FromAccountId} sang {e.ToAccountId} | Số tiền: {e.Amount:N0} VND");
});

var accountService = new AccountService(eventBus);
var analyticsService = new AnalyticsService(eventBus);
var faudDetectionService = new FaudDetectionService(eventBus);

if (accountService.GetAccountsCount() > 0)
{
    Console.WriteLine($"[Hệ thống] Đã khôi phục dữ liệu {accountService.GetAccountsCount()} tài khoản từ file accounts.json.");
}

while (true)
{
    Console.WriteLine("\n================ QUẢN LÝ TÀI KHOẢN (EDA) ================");
    Console.WriteLine("1. Mở tài khoản mới");
    Console.WriteLine("2. Nạp tiền vào tài khoản");
    Console.WriteLine("3. Rút tiền từ tài khoản");
    Console.WriteLine("4. Chuyển tiền (từ A sang B)");
    Console.WriteLine("5. Kiểm tra số dư");
    Console.WriteLine("0. Thoát chương trình");
    Console.WriteLine("=========================================================");
    Console.Write("Chọn chức năng: ");

    var choice = Console.ReadLine();
    if (choice == "0")
    {
        Console.WriteLine("Đã thoát chương trình.");
        break;
    }

    switch (choice)
    {
        case "1":
            Console.Write("Nhập số tài khoản: ");
            var accId = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(accId))
            {
                Console.WriteLine("Số tài khoản không được để trống!");
                break;
            }

            Console.Write("Nhập tên chủ tài khoản: ");
            var owner = Console.ReadLine()?.Trim() ?? "Chưa rõ";

            Console.Write("Nhập số dư ban đầu: ");
            if (!double.TryParse(Console.ReadLine(), out var initBal) || initBal < 0)
            {
                Console.WriteLine("Số dư ban đầu không hợp lệ!");
                break;
            }

            accountService.CreateAccount(accId, owner, initBal);
            break;

        case "2":
            Console.Write("Nhập số tài khoản: ");
            var depAcc = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(depAcc))
            {
                Console.WriteLine("Số tài khoản không được để trống!");
                break;
            }

            Console.Write("Nhập số tiền nạp: ");
            if (!double.TryParse(Console.ReadLine(), out var depAmount) || depAmount <= 0)
            {
                Console.WriteLine("Số tiền nạp không hợp lệ!");
                break;
            }

            accountService.Deposit(depAcc, depAmount);
            break;

        case "3":
            Console.Write("Nhập số tài khoản: ");
            var withAcc = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(withAcc))
            {
                Console.WriteLine("Số tài khoản không được để trống!");
                break;
            }

            Console.Write("Nhập số tiền rút: ");
            if (!double.TryParse(Console.ReadLine(), out var withAmount) || withAmount <= 0)
            {
                Console.WriteLine("Số tiền rút không hợp lệ!");
                break;
            }

            accountService.Withdraw(withAcc, withAmount);
            break;

        case "4":
            Console.Write("Nhập số tài khoản chuyển (A): ");
            var fromAcc = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(fromAcc))
            {
                Console.WriteLine("Số tài khoản chuyển không được để trống!");
                break;
            }

            Console.Write("Nhập số tài khoản nhận (B): ");
            var toAcc = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(toAcc))
            {
                Console.WriteLine("Số tài khoản nhận không được để trống!");
                break;
            }

            Console.Write("Nhập số tiền chuyển: ");
            if (!double.TryParse(Console.ReadLine(), out var transferAmount) || transferAmount <= 0)
            {
                Console.WriteLine("Số tiền chuyển không hợp lệ!");
                break;
            }

            accountService.Transfer(fromAcc, toAcc, transferAmount);
            break;

        case "5":
            Console.Write("Nhập số tài khoản: ");
            var checkAcc = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(checkAcc))
            {
                Console.WriteLine("Số tài khoản không được để trống!");
                break;
            }

            var account = accountService.GetAccount(checkAcc);
            if (account == null)
            {
                Console.WriteLine($"[Lỗi] Không tìm thấy tài khoản {checkAcc}!");
                break;
            }

            Console.WriteLine($"Thông tin tài khoản: {account.AccountId} | Chủ tài khoản: {account.OwnerName} | Số dư: {account.Balance:N0} VND");
            break;

        default:
            Console.WriteLine("Lựa chọn không hợp lệ, vui lòng chọn lại!");
            break;
    }
}
