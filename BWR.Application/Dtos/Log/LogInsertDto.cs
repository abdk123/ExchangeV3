using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BWR.Application.Common;
using BWR.Domain.Model.Logs;
namespace BWR.Application.Dtos.Log
{
    public class LogInsertDto : EntityDto
    {
         public string CloumnName { get; set; }
         public string BeforeUpdate { get; set; }
         public string AfterUpdate { get; set; }
         public DateTime Date { get; set; }
         public string Object { get; set; }
         public int ActionId { get; set; }
         
         public Guid UserId { get; set; }
         
    }
}
