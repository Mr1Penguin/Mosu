using System;
using System.Collections.Generic;
using System.Text;

namespace TenMock
{
    internal class MockData
    {
        internal uint CallCount { get; set; }
        internal object Return { get; set; }
        internal Delegate ReturnCall { get; set; }
    }
}
