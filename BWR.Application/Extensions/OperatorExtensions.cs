using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Extensions
{
   public static class OperatorExtensions
    {
        public static bool EqualrForAny<T>(this T x, params T[] param)
        {
            for (int i = 0; i <param.Length ; i++)
            {
                if (x.Equals(param[i]))
                    return true;
            }
            return false;
        }
    }
}
