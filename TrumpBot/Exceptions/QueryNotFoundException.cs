using System;

namespace TrumpBot.Exceptions
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
