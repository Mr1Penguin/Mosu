using System;
using System.Collections.Generic;
using System.Text;

namespace TenMock
{
    public class IncorrectCallException : Exception
    {
        public IncorrectCallException(string message) : base(message)
        {
        }
    }
}
