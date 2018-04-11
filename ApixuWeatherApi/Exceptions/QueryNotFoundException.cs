using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApixuWeatherApi.Exceptions
{
    public class QueryNotFoundException : Exception
    {
        public QueryNotFoundException(string message) : base(message)
        {
            
        }

        public QueryNotFoundException()
        {
            
        }
    }
}
