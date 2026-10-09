using EDA.Events;
using System.Text.Json;

namespace EDA.Services
{
    public class AnalyticsData
    {
        public double TotalDeposited { get; set; }
        public double TotalWithdrawn { get; set; }
    }

    public class AnalyticsService
    {
        private readonly string _filePath = "analytics.json";
        private double _totalDeposited = 0;
        private double _totalWithdrawn = 0;

        public AnalyticsService(IEventBus _bus)
        {
            LoadFromFile();
            _bus.Subscribe<MoneyDepositedEvent>(HandleDeposited);
            _bus.Subscribe<MoneyWithdrawnEvent>(HandleWithdrawn);
        }

        private void LoadFromFile()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var data = JsonSerializer.Deserialize<AnalyticsData>(json);
                    if (data != null)
                    {
                        _totalDeposited = data.TotalDeposited;
                        _totalWithdrawn = data.TotalWithdrawn;
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
                var data = new AnalyticsData
                {
                    TotalDeposited = _totalDeposited,
                    TotalWithdrawn = _totalWithdrawn
                };
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception)
            {
            }
        }

        private void HandleWithdrawn(MoneyWithdrawnEvent @event)
        {
            _totalWithdrawn += @event.Amount;
            SaveToFile();
            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} [Dịch vụ Thống kê] Tổng tiền đã rút: {_totalWithdrawn:N0} VND");
        }

        private void HandleDeposited(MoneyDepositedEvent @event)
        {
            _totalDeposited += @event.Amount;
            SaveToFile();
            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} [Dịch vụ Thống kê] Tổng tiền đã nạp: {_totalDeposited:N0} VND");
        }
    }
}
