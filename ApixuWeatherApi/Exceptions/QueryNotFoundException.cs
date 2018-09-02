using System;

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
