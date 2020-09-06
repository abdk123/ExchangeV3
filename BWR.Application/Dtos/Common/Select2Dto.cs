using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Common
{
    public class Select2Dto<Key>
    {
        public Key id { get; set; }
        public string text { get; set; }
    }
}
