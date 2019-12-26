using System;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Mosu.Tests
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
        public void RegisterAndCall_FuncArgTFunc_Ok()
        {
            Expression<Func<Obj, int>> e = o => o.FuncArg(2);
            var mock = new Obj();
            mock.Register(e).Returns((int k) => 25 + k);
            Assert.Equal(27, mock.FuncArg(2));

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

        [Fact]
        public void RegisterAndCall_ActAnyOfParentArg_Ok()
        {
            Expression<Action<Obj>> e = o => o.ActWithParent(Arg.AnyOf<Parent>());
            var mock = new Obj();
            mock.Register(e);
            var p = new Parent();
            mock.ActWithParent(p);

            mock.Check(e, 1);
        }

        [Fact]
        public void RegisterAndCall_ActAnyOfChildArg_Ok()
        {
            Expression<Action<Obj>> e = o => o.ActWithParent(Arg.AnyOf<Parent>());
            var mock = new Obj();
            mock.Register(e);
            var p = new Child();
            mock.ActWithParent(p);

            mock.Check(e, 1);
        }

        [Fact]
        public void RegisterAndCall_ActAnyOfWrongArg_Exception()
        {
            Expression<Action<Obj>> e = o => o.ActWithParent(Arg.AnyOf<Child>());
            var mock = new Obj();
            mock.Register(e);
            var p = new Parent();
            mock.ActWithParent(p);

            var exp = Assert.Throws<IncorrectCallException>(() => mock.Check(e, 1));
            var message = "Void ActWithParent(Parent): argument 0 expected [Mosu.Tests.MockTests+Child]AnyOf(), actual Mosu.Tests.MockTests+Parent";
            Assert.Equal(message, exp.Message);
        }

        private class Parent { }
        private class Child : Parent { }

        private class Obj : Mock<Obj/*Or ISomeInterface*/> //, ISomeInterface
        {
            public void ActWithParent(Parent arg) 
            {
                Call(o => o.ActWithParent(arg)); 
            }

            public int Func<T>() 
            { 
                return Call(o => o.Func<T>()); 
            }

            public int FuncArg(int j)
            {
                return Call(o => o.FuncArg(j), j);
            }

            public void Act<T>()
            {
                Call(o => o.Act<T>());
            }
        }
    }
}
