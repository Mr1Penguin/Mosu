using System;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace TenMock.Tests
{
    public class MockTests
    {
        [Fact]
        public void Register_FuncT_Ok()
        {
            Expression<Func<Obj, int>> e = o => o.Func<int>();
            var mock = new Obj();
            mock.Register(e);
            Assert.True(mock.IsRegistered(e));
            Assert.Equal(1, mock.CountOfMocks());
        }

        [Fact]
        public void Register_TwoFuncT_Ok()
        {
            Expression<Func<Obj, int>> e = o => o.Func<int>();
            Expression<Func<Obj, int>> e2 = o => o.Func<string>();
            var mock = new Obj();
            mock.Register(e);
            mock.Register(e2);
            Assert.True(mock.IsRegistered(e));
            Assert.True(mock.IsRegistered(e2));
            Assert.Equal(2, mock.CountOfMocks());
        }

        [Fact]
        public void RegisterAndCall_FuncT_Ok()
        {
            Expression<Func<Obj, int>> e = o => o.Func<int>();
            var mock = new Obj();
            mock.Register(e);
            mock.Func<int>();

            mock.Check(e, 1);
        }

        [Fact]
        public void RegisterAndCall_FuncTIncorrectCount_Exception()
        {
            Expression<Func<Obj, int>> e = o => o.Func<int>();
            var mock = new Obj();
            mock.Register(e);
            mock.Func<int>();

            Assert.Throws<IncorrectCallException>(() => mock.Check(e, 2));
        }

        private class Obj : Mock<Obj>
        {
            public int Func<T>() { 
                Call(o => o.Func<T>()); 
                return default;
            }
        }
    }
}
