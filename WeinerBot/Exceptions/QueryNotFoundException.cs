using System;

namespace WeinerBot.Exceptions
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
