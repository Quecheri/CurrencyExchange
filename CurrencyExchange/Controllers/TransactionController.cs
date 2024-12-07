using Currency_exchange.Models;
using CurrencyExchange.Data;
using CurrencyExchange.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CurrencyExchange.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TransactionController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Exchange()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wallet = await _context.UserWallets
                .Include(w => w.Currency)
                .Where(w => w.UserId == user.Id)
                .ToListAsync();

            var currencies = await _context.Currencies.ToListAsync();

            ViewBag.UserWallet = wallet;
            ViewBag.Currencies = currencies;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Exchange(int fromCurrencyId, int toCurrencyId, string amountStr)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wallet = await _context.UserWallets
                .Include(w => w.Currency)
                .Where(w => w.UserId == user.Id)
                .ToListAsync();

            var currencies = await _context.Currencies.ToListAsync();

            ViewBag.UserWallet = wallet;
            ViewBag.Currencies = currencies;
            ViewData["fromCurrencyId"] = fromCurrencyId.ToString();
            ViewData["toCurrencyId"] = toCurrencyId.ToString();
            ViewData["amountStr"] = amountStr;


            amountStr = amountStr.Replace(",", ".");
            if (!decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
            {
                ModelState.AddModelError("amountStr", "Invalid amount format. Please use a valid number format.");
                return View();  // Nie przekierowuj, tylko zwróć widok z błędami
            }

            if (fromCurrencyId == toCurrencyId)
            {
                ModelState.AddModelError("fromCurrencyId", "Cannot exchange the same currency.");
                return View();  // Nie przekierowuj, tylko zwróć widok z błędami
            }

            if (amount <= 0)
            {
                ModelState.AddModelError("amountStr", "Amount must be greater than zero.");
                return View();  // Nie przekierowuj, tylko zwróć widok z błędami
            }

            var fromCurrency = await _context.Currencies.FindAsync(fromCurrencyId);
            var toCurrency = await _context.Currencies.FindAsync(toCurrencyId);

            var walletFrom = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.CurrencyId == fromCurrencyId);

            var walletTo = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.CurrencyId == toCurrencyId);

            if (walletFrom == null || walletFrom.Amount < amount)
            {
                ModelState.AddModelError("amountStr", "Insufficient funds.");
                return View();  // Nie przekierowuj, tylko zwróć widok z błędami
            }

            decimal exchangeRate = toCurrency.CurrentRate / fromCurrency.CurrentRate;
            decimal exchangedAmount = amount * exchangeRate;

            walletFrom.Amount -= amount;

            if (walletTo != null)
            {
                walletTo.Amount += exchangedAmount;
            }
            else
            {
                _context.UserWallets.Add(new UserWallet
                {
                    User = user,
                    UserId = user.Id,
                    Currency = toCurrency,
                    CurrencyId = toCurrencyId,
                    Amount = exchangedAmount,
                });
            }

            _context.Transactions.Add(new Transaction
            {
                UserId = user.Id,
                Type = TransactionType.Exchange.ToString(),
                FromCurrencyId = fromCurrencyId,
                ToCurrencyId = toCurrencyId,
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                FromAmount = amount,
                ToAmount = exchangedAmount,
                ExchangeRate = exchangeRate,
                TransactionDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Wallet");
        }
        [HttpGet]
        [Route("api/rate")]
        public async Task<IActionResult> GetExchangeRate(int fromCurrencyId, int toCurrencyId)
        {
            var fromCurrency = await _context.Currencies.FindAsync(fromCurrencyId);
            var toCurrency = await _context.Currencies.FindAsync(toCurrencyId);

            if (fromCurrency == null || toCurrency == null)
            {
                return NotFound("One of the currencies was not found.");
            }

            var exchangeRate = toCurrency.CurrentRate / fromCurrency.CurrentRate;
            return Json(exchangeRate);
        }


        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var transactions = await _context.Transactions
                .Where(t => t.UserId == user.Id)
                .Include(t => t.FromCurrency)  
                .Include(t => t.ToCurrency)    
                .ToListAsync();

            return View(transactions);
        }
    }
}