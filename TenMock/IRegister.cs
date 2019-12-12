using System;
using System.Collections.Generic;
using System.Text;

namespace TenMock
{
    public interface IRegister<T>
    {
        void Returns(T val);
        void Returns(Func<T> func);
        void Returns<T1>(Func<T1, T> func);
    }
}
