using Currency_exchange.Models;
using CurrencyExchange.Data;
using CurrencyExchange.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CurrencyExchange.Controllers
{
    public class WalletController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public WalletController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }


            var userWallet = await _context.UserWallets
                .Include(uw => uw.Currency)
                .Where(uw => uw.UserId == user.Id)
                .ToListAsync();


            ViewBag.Currencies = await _context.Currencies.ToListAsync();

            return View(userWallet);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWallet(int currencyId, string amount)
        {
            amount = amount.Replace(",", ".");

            if (!decimal.TryParse(amount, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedAmount))
            {
                ModelState.AddModelError(string.Empty, "Invalid amount format. Please use a valid number format.");
                return View();
            }
            if (parsedAmount <= 0)
            {
                ModelState.AddModelError(string.Empty, "Amount must be greater than zero.");
                return View();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var currency = await _context.Currencies.FindAsync(currencyId);
            if (currency == null)
            {
                return BadRequest("Currency not found.");
            }
            var walletEntry = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.CurrencyId == currencyId);


            decimal currentAmount = walletEntry?.Amount ?? 0;
            decimal currenctRate = walletEntry?.Currency.CurrentRate ?? 0;
            if (walletEntry != null)
            {
                walletEntry.Amount += parsedAmount;
            }
            else
            {
                _context.UserWallets.Add(new UserWallet
                {
                    User = user,
                    UserId = user.Id,
                    Currency = currency,
                    Amount = parsedAmount,
                    CurrencyId = currencyId,
                });
            }
            _context.Transactions.Add(new Transaction
            {
                UserId = user.Id,
                Type = TransactionType.Deposit.ToString(),
                FromCurrencyId = currencyId,
                FromCurrency = currency,
                ToCurrency  = currency,
                ToCurrencyId = currencyId,  
                FromAmount = currentAmount, 
                ToAmount = currentAmount + parsedAmount,
                ExchangeRate = 1,           
                TransactionDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
