using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Extensions
{
    public static class DoubleExtension
    {
        public static string InDollars(this double d)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            return String.Format(culture, "{0:C2}", d);
        }
    }
}
