using System;

namespace Htoytp.Server
{
    public class BadRequestException : Exception
    {
        public BadRequestException() 
        {
            
        }

        public BadRequestException(string reason)
            : base(reason)
        {
            
        }
    }
}