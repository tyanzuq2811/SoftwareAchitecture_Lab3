using EDA.Events;

namespace EDA.Services
{
    public class FaudDetectionService
    {
        public FaudDetectionService(IEventBus bus)
        {
            bus.Subscribe<MoneyWithdrawnEvent>(HandleWithdrawn);
        }

        private void HandleWithdrawn(MoneyWithdrawnEvent @event)
        {
            if (@event.Amount > 10000)
            {
                Console.WriteLine($"\n[Dịch vụ Phát hiện Gian lận] Cảnh báo: Phát hiện giao dịch rút tiền bất thường từ tài khoản {@event.AccountId} với số tiền {@event.Amount:N0} VND (> 10.000)!");
            }
        }
    }

    public class FraudDetectionService : FaudDetectionService
    {
        public FraudDetectionService(IEventBus bus) : base(bus)
        {
        }
    }
}
