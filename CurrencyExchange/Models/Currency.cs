using System;

namespace Currency_exchange.Models
{
    public class Currency
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal CurrentRate { get; set; }
        public DateTime RateDate { get; set; }
    }
}