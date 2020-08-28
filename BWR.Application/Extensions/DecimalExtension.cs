using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Extensions
{
    public static class DecimalExtension
    {
        public static string CurrencyFormat(this decimal number, bool positive = true)
        {
            if (positive)
            {
                number = Math.Abs(number);
            }
            string y = string.Format("{0:N}", number);
            var w = y.Split('.');
            int z = 0;
            Console.WriteLine(w[1][0]);
            Console.WriteLine(w[1][1]);
            if (w[1][1] == '0')
            {
                string f = w[1][0].ToString();
                z = Convert.ToInt32(f);
            }
            else
            {
                z = Convert.ToInt32(w[1]);
            }
            if (z == 0)
                return w[0];

            return w[0] + '.' + z;
        }
        public static string CurrencyBalnceFormat(this decimal number)
        {
            string status = number > 0 ? "له" : "عليه";
            if (number == 0)
                return null;
            return status;
        }
        public static string CurrencyFormat(this decimal? number, bool positve = true)
        {
            decimal nonNullableNumber = number ?? 0;
            return nonNullableNumber.CurrencyFormat(positve);
        }
    }
}
