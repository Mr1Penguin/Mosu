using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Mosu
{
    abstract public class Mock<T>
    {
        private Dictionary<string, MockData> mocks;

        public Mock()
        {
            mocks = new Dictionary<string, MockData>();
        }

        public IRegister<TRes> Register<TRes>(Expression<Func<T, TRes>> expression)
        {
            this.RegisterExpression(expression);
            var key = KeyGenerator.GetKey(expression);
            mocks[key].Return = default(TRes);
            return new RegisterImpl<TRes>() { Key = key, Ref = this };
        }

        public void Check<TRes>(Expression<Func<T, TRes>> expression, uint count)
        {
            CheckExpression(expression, count);
        }

        protected TRes Call<TRes>(Expression<Func<T, TRes>> expression, params object[] args)
        {
            CallExpression(expression);

            var key = KeyGenerator.GetKey(expression);
            var callback = mocks[key].Callback;
            if (callback != null)
            {
                callback.DynamicInvoke(args);
            }

            var func = mocks[key].ReturnCall;
            if (func != null)
            {
                return (TRes)func.DynamicInvoke(args);
            }

            return (TRes)mocks[key].Return;
        }

        public void Register(Expression<Action<T>> expression)
        {
            this.RegisterExpression(expression);
        }

        public void Check(Expression<Action<T>> expression, uint count)
        {
            CheckExpression(expression, count);
        }

        protected void Call(Expression<Action<T>> expression)
        {
            CallExpression(expression);
        }

        public bool IsRegistered(Expression<Action<T>> expression) => this.mocks.ContainsKey(KeyGenerator.GetKey(expression));

        public bool IsRegistered<TRes>(Expression<Func<T, TRes>> expression) => this.mocks.ContainsKey(KeyGenerator.GetKey(expression));

        public int CountOfMocks() => mocks.Count;

        private void RegisterExpression<TDeleg>(Expression<TDeleg> expression)
        {
            if (!IsExpressionCorrect(expression))
            {
                throw new ArgumentException("expression must be a call");
            }

            string key = KeyGenerator.GetKey(expression);
            mocks.Remove(key);
            mocks.Add(key, new MockData());

            mocks[key].Arguments = ((MethodCallExpression)expression.Body).Arguments.Select(ExtractValue).ToArray();
        }

        private void CheckExpression<TDeleg>(Expression<TDeleg> expression, uint count)
        {
            var key = KeyGenerator.GetKey(expression);
            var actual = mocks[key].CallCount;
            if (count != actual)
            {
                throw new IncorrectCallException($"{key}: expected {count}, actual {actual}");
            }

            var method = (MethodCallExpression)expression.Body;
            var args = method.Arguments;

            for (var j = 0; j < count; ++j)
            {
                for (var i = 0; i < args.Count; ++i)
                {
                    var res = CheckArgument(args[i], mocks[key].ActualArguments[j][i]);
                    if (!res)
                    {
                        throw new IncorrectCallException($"{key}: argument {i} expected {ExpressionToString(args[i])}, actual {mocks[key].ActualArguments[j][i]}");
                    }
                }
            }
        }

        private string ExpressionToString(Expression expression)
        {
            if (expression is MethodCallExpression e && e.Method.DeclaringType.Name == nameof(Arg))
            {
                return $"[{e.Method.ReturnType}]{e.Method.Name}()";
            }

            return expression.ToString();
        }

        private void CallExpression<TDeleg>(Expression<TDeleg> expression)
        {
            var key = KeyGenerator.GetKey(expression);
            if (!mocks.ContainsKey(key))
            {
                return;
            }

            mocks[key].CallCount++;            
            mocks[key].ActualArguments.Add(((MethodCallExpression)expression.Body).Arguments.Select(ExtractValue).ToArray());
        }

        private static bool IsExpressionCorrect<TDeleg>(Expression<TDeleg> expression) => expression.Body.NodeType == ExpressionType.Call;

        private static object ExtractValue(Expression expression)
        {
            switch (expression)
            {
                case ConstantExpression e:
                    return e.Value;
                case MethodCallExpression e:
                    if (e.Method.DeclaringType.Name != nameof(Arg))
                    {
                        throw new ArgumentException();
                    }
                    return e;
                case MemberExpression e:
                    return Expression.Lambda(e).Compile().DynamicInvoke();
                default:
                    throw new ArgumentException();
            }
        }

        private static bool CheckArgument(Expression expression, object argument)
        {
            switch (expression)
            {
                case ConstantExpression e:
                    if (!e.Type.IsInstanceOfType(argument))
                        return false;
                    return e.Value.Equals(argument);
                case MethodCallExpression e:
                    return CheckArgArgument(e, argument);
                case MemberExpression e:
                    var res = Expression.Lambda(e).Compile().DynamicInvoke();
                    return res.Equals(argument);
                default:
                    throw new ArgumentException();
            }
        }

        private static bool CheckArgArgument(MethodCallExpression expression, object argument)
        {
            switch(expression.Method.Name)
            {
                case "AnyOf":
                    return expression.Method.ReturnType.IsInstanceOfType(argument);
                default:
                    throw new ArgumentException();
            }
        }

        private class RegisterImpl<TRes> : IRegister<TRes>
        {
            public Mock<T> Ref { get; set; }
            public string Key { get; set; }

            public IRegister<TRes> Returns(TRes val)
            {
                Ref.mocks[Key].Return = val;
                return this;
            }

            public IRegister<TRes> Returns(Func<TRes> func)
            {
                Ref.mocks[Key].ReturnCall = func;
                return this;
            }

            public IRegister<TRes> Returns<T1>(Func<T1, TRes> func)
            {
                Ref.mocks[Key].ReturnCall = func;
                return this;
            }

            public IRegister<TRes> Callback<T1>(Action<T1> action)
            {
                Ref.mocks[Key].Callback = action;
                return this;
            }

            public IRegister<TRes> Callback<T1, T2>(Action<T1, T2> action)
            {
                Ref.mocks[Key].Callback = action;
                return this;
            }
        }
    }
}
