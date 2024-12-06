namespace CurrencyExchange.Services
{
    using Microsoft.Extensions.Hosting;
    using System.Threading;
    using System.Threading.Tasks;

    public class CurrencyUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public CurrencyUpdateService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var currencyService = scope.ServiceProvider.GetRequiredService<CurrencyService>();
                    await currencyService.UpdateCurrencyRatesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }

}
