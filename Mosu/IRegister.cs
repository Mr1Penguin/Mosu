using System;
using System.Collections.Generic;
using System.Text;

namespace Mosu
{
    public interface IRegister<T>
    {
        IRegister<T> Returns(T val);
        IRegister<T> Returns(Func<T> func);
        IRegister<T> Returns<T1>(Func<T1, T> func);
        IRegister<T> Callback<T1>(Action<T1> action);
        IRegister<T> Callback<T1,T2>(Action<T1, T2> action);
    }
}
