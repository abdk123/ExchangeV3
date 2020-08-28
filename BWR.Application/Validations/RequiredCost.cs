using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Validations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredCostAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var cost = (int)value;
            if (cost == 0)
                return true;

            ErrorMessage = "القيمة مطلوبة";
            return false;
        }
    }
}
