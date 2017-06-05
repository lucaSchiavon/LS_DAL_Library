using System;
using System.Collections.Generic;
using System.Text;

namespace it.itryframework2.exception
{
    public class ITryFrameworkException : Exception
    {
        public ITryFrameworkException () : base() 
        {
            
        }

        public ITryFrameworkException(string message)
            : base(message)
        {
            
        }
    }
}
