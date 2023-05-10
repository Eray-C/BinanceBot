using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceBot
{
    public class Price
    {

        public string symbol { get; set; }
        public double priceChange { get; set; }
        public double priceChangePercent { get; set; }

        public double lastPrice  { get; set; }

    }
}
