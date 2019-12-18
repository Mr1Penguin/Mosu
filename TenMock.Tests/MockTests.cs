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
        public void RegisterAndCall_FuncTDefault_Ok()
        {
            Expression<Func<Obj, int>> e = o => o.Func<int>();
            var mock = new Obj();
            mock.Register(e);
            Assert.Equal(default, mock.Func<int>());

            mock.Check(e, 1);
        }

        [Fact]
        public void RegisterAndCall_FuncTVal_Ok()
        {
            Expression<Func<Obj, int>> e = o => o.Func<int>();
            var mock = new Obj();
            mock.Register(e).Returns(42);
            Assert.Equal(42, mock.Func<int>());

            mock.Check(e, 1);
        }

        [Fact]
        public void RegisterAndCall_FuncTFunc_Ok()
        {
            Expression<Func<Obj, int>> e = o => o.Func<int>();
            var mock = new Obj();
            mock.Register(e).Returns(() => 25);
            Assert.Equal(25, mock.Func<int>());

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

        [Fact]
        public void RegisterAndCall_FuncTCorrectExactArgValue_Ok()
        {
            Expression<Func<Obj, int>> e = o => o.FuncArg(42);
            var mock = new Obj();
            mock.Register(e);
            mock.FuncArg(42);

            mock.Check(e, 1);
        }

        [Fact]
        public void RegisterAndCall_FuncTIncorrectExactArgValue_Ok()
        {
            Expression<Func<Obj, int>> e = o => o.FuncArg(42);
            var mock = new Obj();
            mock.Register(e);
            mock.FuncArg(43);

            Assert.Throws<IncorrectCallException>(() => mock.Check(e, 1));
        }

        [Fact]
        public void Register_ActT_Ok()
        {
            Expression<Action<Obj>> e = o => o.Act<int>();
            var mock = new Obj();
            mock.Register(e);
            Assert.True(mock.IsRegistered(e));
            Assert.Equal(1, mock.CountOfMocks());
        }

        [Fact]
        public void Register_TwoActT_Ok()
        {
            Expression<Action<Obj>> e = o => o.Act<int>();
            Expression<Action<Obj>> e2 = o => o.Act<string>();
            var mock = new Obj();
            mock.Register(e);
            mock.Register(e2);
            Assert.True(mock.IsRegistered(e));
            Assert.True(mock.IsRegistered(e2));
            Assert.Equal(2, mock.CountOfMocks());
        }

        [Fact]
        public void RegisterAndCall_ActT_Ok()
        {
            Expression<Action<Obj>> e = o => o.Act<int>();
            var mock = new Obj();
            mock.Register(e);
            mock.Act<int>();

            mock.Check(e, 1);
        }

        [Fact]
        public void RegisterAndCall_ActTIncorrectCount_Exception()
        {
            Expression<Action<Obj>> e = o => o.Act<int>();
            var mock = new Obj();
            mock.Register(e);
            mock.Act<int>();

            Assert.Throws<IncorrectCallException>(() => mock.Check(e, 2));
        }

        private class Obj : Mock<Obj>
        {
            public int Func<T>() 
            { 
                return Call(o => o.Func<T>()); 
            }

            public int FuncArg(int j)
            {
                return Call(o => o.FuncArg(j));
            }

            public void Act<T>()
            {
                Call(o => o.Act<T>());
            }
        }
    }
}
