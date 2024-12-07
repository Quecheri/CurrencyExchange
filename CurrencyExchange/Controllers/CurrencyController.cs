using Currency_exchange.Models;
using CurrencyExchange.Data;
using CurrencyExchange.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;


namespace CurrencyExchange.Controllers
{


    public class CurrencyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CurrencyService _currencyService;

        public CurrencyController(AppDbContext context)
        {
            _context = context;
            _currencyService = new CurrencyService(context);
        }

        public IActionResult Index()
        {
            var prioritizedCurrencies = new[] { "PLN", "USD", "EUR", "JPY", "GBP", "CHF", "CZK","NOK","DKK","SEK","HUF"};

            var currencies = _context.Currencies
                .ToList()
                .OrderBy(c => Array.IndexOf(prioritizedCurrencies, c.Code) == -1 ? int.MaxValue : Array.IndexOf(prioritizedCurrencies, c.Code)) // Priorytetowe waluty
                .ThenBy(c => c.Name) // Reszta alfabetycznie
                .ToList();

            return View(currencies);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateRates()
        {
            await _currencyService.UpdateCurrencyRatesAsync();
            return RedirectToAction("Index");
        }
    }

}
