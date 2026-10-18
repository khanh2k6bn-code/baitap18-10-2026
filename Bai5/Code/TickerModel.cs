using System;

namespace CryptoTickerApp
{
    public class TickerModel
    {
        public string TimeStamp { get; set; }
        public decimal Price { get; set; }
        public string Trend { get; set; } // Tăng / Giảm
    }
}
