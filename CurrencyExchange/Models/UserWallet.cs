using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Currency_exchange.Models
{
    public class UserWallet
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
        public decimal Amount { get; set; }
    }
}
