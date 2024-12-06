using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Currency_exchange.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}