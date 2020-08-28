using BWR.ShareKernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Domain.Model.Common
{
    public class PhoneBase: Entity
    {
        public string Phone { get; set; }
        public bool IsEnabled { get; set; }
    }
}
