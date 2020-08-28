using System;
using BWR.Application.Common;

namespace BWR.Application.Dtos.Log
{
    public class LogUpdateDto : EntityDto
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
