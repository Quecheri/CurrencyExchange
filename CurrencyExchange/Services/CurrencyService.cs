namespace CurrencyExchange.Services
{
    using Currency_exchange.Models;
    using CurrencyExchange.Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json.Linq;
    using System.Net.Http;

    public class CurrencyService
    {
        private readonly AppDbContext _context;

        public CurrencyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpdateCurrencyRatesAsync()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync("https://api.exchangerate-api.com/v4/latest/USD");
                var data = JObject.Parse(response);

                var rates = data["rates"] as JObject;
                if (rates != null)
                {
                    foreach (var rate in rates.Properties())
                    {
                        var currencyCode = rate.Name;
                        var currencyRate = (decimal)rate.Value;

                        var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == currencyCode);
                        if (currency != null)
                        {
                            currency.CurrentRate = currencyRate;
                            currency.RateDate = DateTime.UtcNow;
                        }
                        else
                        {
                            _context.Currencies.Add(new Currency
                            {
                                Code = currencyCode,
                                Name = currencyCode,
                                CurrentRate = currencyRate,
                                RateDate = DateTime.UtcNow
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
