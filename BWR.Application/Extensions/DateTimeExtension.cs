using System;

namespace BWR.Application.Extensions
{
    public class DateTimeExtension
    {
        public static string DateTimeNow()
        {
            return DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Date;
        }
    }
}
