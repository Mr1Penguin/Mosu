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
            mocks[GetKey(expression)].Return = default(TRes);
            return new RegisterImpl<TRes>() { Key = GetKey(expression), Ref = this };
        }

        public void Check<TRes>(Expression<Func<T, TRes>> expression, uint count)
        {
            CheckExpression(expression, count);
        }

        protected TRes Call<TRes>(Expression<Func<T, TRes>> expression, params object[] args)
        {
            CallExpression(expression);

            var func = mocks[GetKey(expression)].ReturnCall;
            if (func != null)
            {
                return (TRes)func.DynamicInvoke(args);
            }

            return (TRes)mocks[GetKey(expression)].Return;
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

        public bool IsRegistered(Expression<Action<T>> expression) => this.mocks.ContainsKey(GetKey(expression));

        public bool IsRegistered<TRes>(Expression<Func<T, TRes>> expression) => this.mocks.ContainsKey(GetKey(expression));

        public int CountOfMocks() => mocks.Count;

        private void RegisterExpression<TDeleg>(Expression<TDeleg> expression)
        {
            if (!IsExpressionCorrect(expression))
            {
                throw new ArgumentException("expression must be a call"); 
            }

            string key = GetKey(expression);
            mocks.Remove(key);
            mocks.Add(key, new MockData());

            mocks[key].Arguments = ((MethodCallExpression)expression.Body).Arguments.Select(ExtractValue).ToArray();
        }

        private void CheckExpression<TDeleg>(Expression<TDeleg> expression, uint count)
        {
            var key = GetKey(expression);
            var actual = mocks[key].CallCount;
            if (count != actual)
            {
                throw new IncorrectCallException($"{GetKey(expression)}: expected {count}, actual {actual}");
            }

            var method = (MethodCallExpression)expression.Body;
            var args = method.Arguments;

            for (int j = 0; j < count; ++j)
            {
                for (int i = 0; i < args.Count; ++i)
                {
                    var res = CheckArgument(args[i], mocks[GetKey(expression)].ActualArguments[j][i]);
                    if (!res)
                    {
                        throw new IncorrectCallException($"{GetKey(expression)}: argument {i} expected {args[i]}, actual {mocks[GetKey(expression)].ActualArguments[j][i]}");
                    }
                }
            }
        }

        private void CallExpression<TDeleg>(Expression<TDeleg> expression)
        {
            var key = GetKey(expression);
            if (!mocks.ContainsKey(key))
            {
                return;
            }

            mocks[key].CallCount++;            
            mocks[key].ActualArguments.Add(((MethodCallExpression)expression.Body).Arguments.Select(ExtractValue).ToArray());
        }

        private static string GetKey<TDeleg>(Expression<TDeleg> expression)
        {
            var body = (MethodCallExpression)expression.Body;
            var method = body.Method;
            var sb = new StringBuilder();
            sb.Append(method.ReturnType.Name).Append(" ").Append(method.Name);
            AddGenericArguments(sb, method.GetGenericArguments());
            AddArguments(sb, body.Arguments);
            return sb.ToString();
        }

        private static void AddGenericArguments(StringBuilder sb, Type[] types)
        {
            if (types.Length == 0)
            {
                return;
            }

            sb.Append("<");
            foreach (var type in types)
            {
                sb.Append(type.Name).Append(", ");
            }

            sb.Length -= 2;
            sb.Append(">");
        }

        private static void AddArguments(StringBuilder sb, IReadOnlyCollection<Expression> Arguments)
        {
            sb.Append("(");
            foreach (var param in Arguments)
            {
                switch (param)
                {
                    case ConstantExpression e:
                        sb.Append(e.Type.Name);
                        break;
                    case MethodCallExpression e:
                        sb.Append(e.Method.ReturnType.Name);
                        break;
                    default:
                        sb.Append(param.Type.Name);
                        break;
                }
                sb.Append(", ");
            }

            if (Arguments.Count != 0)
            {
                sb.Length -= 2;
            }

            sb.Append(")");
        }

        private static bool IsExpressionCorrect<TDeleg>(Expression<TDeleg> expression) => expression.Body.NodeType == ExpressionType.Call;

        private static object ExtractValue(Expression expression)
        {
            switch (expression)
            {
                case ConstantExpression e:
                    return e.Value;
                case MethodCallExpression e:
                    throw new NotImplementedException();
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
                case MemberExpression e:
                    var res = Expression.Lambda(e).Compile().DynamicInvoke();
                    return res.Equals(argument);
                default:
                    throw new ArgumentException();
            }
        }

        private class RegisterImpl<TRes> : IRegister<TRes>
        {
            public Mock<T> Ref { get; set; }
            public string Key { get; set; }

            public void Returns(TRes val)
            {
                Ref.mocks[Key].Return = val;
            }

            public void Returns(Func<TRes> func)
            {
                Ref.mocks[Key].ReturnCall = func;
            }

            public void Returns<T1>(Func<T1, TRes> func)
            {
                Ref.mocks[Key].ReturnCall = func;
            }
        }
    }
}
