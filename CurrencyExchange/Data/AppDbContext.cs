using Currency_exchange.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Data
{


    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options){}
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CurrencyExchange;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserLogin<string>>()
       .HasNoKey();
            modelBuilder.Entity<IdentityUserRole<string>>()
      .HasNoKey();
            modelBuilder.Entity<IdentityUserClaim<string>>()
      .HasNoKey();
            modelBuilder.Entity<IdentityUserToken<string>>()
     .HasNoKey();

            modelBuilder.Entity<UserWallet>()
               .HasOne(w => w.User)
               .WithMany()
               .HasForeignKey(w => w.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserWallet>()
                .HasOne(w => w.Currency)
                .WithMany()
                .HasForeignKey(w => w.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.FromCurrency)
                .WithMany()
                .HasForeignKey(t => t.FromCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ToCurrency)
                .WithMany()
                .HasForeignKey(t => t.ToCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<Currency>()
                 .Property(c => c.CurrentRate)
                 .HasPrecision(18, 12);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.ExchangeRate)
                .HasPrecision(18, 10);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.FromAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.ToAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<UserWallet>()
                .Property(u => u.Amount)
                .HasPrecision(18, 2);

        }
    }
}
