using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Common
{
    public class DataTablesDto
    {
        public DataTablesDto(int draw, IEnumerable data, int recordsFiltered, int recordsTotal)
        {
            this.data = data;
            this.draw = draw;
            this.recordsFiltered = recordsFiltered;
            this.recordsTotal = recordsTotal;
        }
        public int draw { get; }
        public IEnumerable data { get; set; }
        public int recordsTotal { get; }
        public int recordsFiltered { get; }
    }
}
