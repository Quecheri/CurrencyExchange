namespace CurrencyExchange.Services
{
    using Currency_exchange.Models;
    using CurrencyExchange.Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json.Linq;
    using System.Net.Http;

    public class CurrencyService
    {

        private readonly string CurrencySourceNBP = "https://api.nbp.pl/api/exchangerates/tables/A?format=Json";
        private readonly string CurrencySourceNBP2 = "https://api.nbp.pl/api/exchangerates/tables/B?format=Json";
        private readonly string CurrencySourceUSD = "https://api.exchangerate-api.com/v4/latest/USD";


        private readonly bool NBPApi=true;


        private readonly AppDbContext _context;

        public CurrencyService(AppDbContext context)
        {
            _context = context;
        }

        private async void ClearDatabaseDO_NOT_USE()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM UserWallets");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Currencies");
        }
        public async Task UpdateCurrencyRatesAsync()
        {

            using (var client = new HttpClient())
            {
                
                if (NBPApi)
                {
                    var tasks = new[]
                    {
                        client.GetStringAsync(CurrencySourceNBP),
                        client.GetStringAsync(CurrencySourceNBP2)
                    };
                    var responses = await Task.WhenAll(tasks);
                    foreach (string response in responses)
                    {
                        var data = JArray.Parse(response);

                        foreach (var item in data)
                        {
                            var rates = item["rates"] as JArray;
                            if (rates != null)
                            {
                                foreach (var rate in rates)
                                {
                                    var currencyCode = (string)rate["code"];
                                    var currencyName = (string)rate["currency"];
                                    var currencyRate = (decimal)rate["mid"];

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
                                            Name = currencyName,
                                            CurrentRate = currencyRate,
                                            RateDate = DateTime.UtcNow
                                        });
                                    }
                                }
                            }
                        }
                    }

                }
                else
                {
                    string response = await client.GetStringAsync(CurrencySourceUSD);
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
                    }
                }
                    await _context.SaveChangesAsync();
            }
        }
    }
}
