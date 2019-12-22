using System;
using System.Collections.Generic;
using System.Text;

namespace Mosu
{
    internal class MockData
    {
        internal MockData()
        {
            ActualArguments = new List<object[]>();
        }
        internal uint CallCount { get; set; }
        internal object Return { get; set; }
        internal Delegate ReturnCall { get; set; }
        internal object[] Arguments { get; set; }
        internal List<object[]> ActualArguments { get; set; }
    }
}
