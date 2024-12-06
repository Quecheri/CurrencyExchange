using Currency_exchange.Models;
using CurrencyExchange.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Controllers
{
    public class WalletController : Controller
    {
        private readonly AppDbContext _context;

        public WalletController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Pobieranie aktualnego użytkownika
            var userId = User. id; // Załóżmy, że nazwa użytkownika jest unikalnym identyfikatorem
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Pobieranie portfela użytkownika
            var userWallet = await _context.UserWallets
                .Include(uw => uw.Currency) // Dołączenie informacji o walucie
                .Where(uw => uw.UserId == userId)
                .ToListAsync();

            // Pobranie listy dostępnych walut do ViewBag
            ViewBag.Currencies = await _context.Currencies.ToListAsync();

            return View(userWallet);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWallet(int currencyId, decimal amount)
        {
            var userId = User.Identity.Name;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var walletEntry = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId && w.CurrencyId == currencyId);

            if (walletEntry != null)
            {
                walletEntry.Amount += amount;
            }
            else
            {
                _context.UserWallets.Add(new UserWallet
                {
                    UserId = userId,
                    CurrencyId = currencyId,
                    Amount = amount
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
