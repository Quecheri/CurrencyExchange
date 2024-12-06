using Currency_exchange.Models;
using CurrencyExchange.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpPost]
        public async Task<IActionResult> Exchange(int fromCurrencyId, int toCurrencyId, decimal amount)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var fromCurrency = await _context.Currencies.FindAsync(fromCurrencyId);
            var toCurrency = await _context.Currencies.FindAsync(toCurrencyId);

            var userWallet = await _context.UserWallets.FirstOrDefaultAsync(w =>
                w.UserId == user.Id && w.CurrencyId == fromCurrencyId);

            if (fromCurrency == null || toCurrency == null || userWallet == null || userWallet.Amount < amount)
            {
                return BadRequest("Invalid transaction.");
            }

            var exchangeRate = fromCurrency.CurrentRate / toCurrency.CurrentRate;
            var toAmount = amount * exchangeRate;
            userWallet.Amount -= amount;

            var toWallet = await _context.UserWallets.FirstOrDefaultAsync(w =>
                w.UserId == user.Id && w.CurrencyId == toCurrencyId);

            if (toWallet != null)
            {
                toWallet.Amount += toAmount;
            }
            else
            {
                _context.UserWallets.Add(new UserWallet
                {
                    UserId = user.Id,
                    CurrencyId = toCurrencyId,
                    Amount = toAmount
                });
            }

            _context.Transactions.Add(new Transaction
            {
                UserId = user.Id,
                FromCurrencyId = fromCurrencyId,
                ToCurrencyId = toCurrencyId,
                FromAmount = amount,
                ToAmount = toAmount,
                ExchangeRate = exchangeRate,
                TransactionDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok("Transaction successful.");
        }

        [HttpPost]
        public async Task<IActionResult> AddToWallet(int currencyId, decimal amount)
        {
            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero.");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var walletEntry = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.CurrencyId == currencyId);

            if (walletEntry == null)
            {
                _context.UserWallets.Add(new UserWallet
                {
                    UserId = user.Id,
                    CurrencyId = currencyId,
                    Amount = amount
                });
            }
            else
            {
                walletEntry.Amount += amount;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Wallet");
        }

        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            var fromCurrency = await _context.Currencies.FindAsync(fromCurrencyId);
            var toCurrency = await _context.Currencies.FindAsync(toCurrencyId);

            var transactions = await _context.Transactions
                .Where(t => t.UserId == user.Id)
                .Include(t => FromCurrency)  
                .Include(t => ToCurrency)    
                .ToListAsync();

            return View(transactions);
        }
    }
}