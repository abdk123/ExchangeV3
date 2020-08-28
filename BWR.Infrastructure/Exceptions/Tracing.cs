using BWR.ShareKernel.Exceptions;
using System;
using System.Threading.Tasks;

namespace BWR.Infrastructure.Exceptions
{
    public class Tracing
    {

        public static void SaveException(Exception ex)
        {
            Task.Factory.StartNew(() =>
            {
                
            });
        }
    }
}
