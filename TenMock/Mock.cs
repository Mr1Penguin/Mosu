using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace TenMock
{
    abstract public class Mock<T>
    {
        private Dictionary<string, uint> mocks;

        public Mock()
        {
            mocks = new Dictionary<string, uint>();
        }

        public Mock<T> Register<TRes>(Expression<Func<T, TRes>> expression)
        {
            this.RegisterExpression(expression);
            return this;
        }

        public void Check<TRes>(Expression<Func<T, TRes>> expression, uint count)
        {
            var actual = mocks[GetKey(expression)];
            if (count != actual)
            {
                throw new IncorrectCallException($"{GetKey(expression)}: expected {count}, actual {actual}");
            }
        }

        protected void Call<TRes>(Expression<Func<T, TRes>> expression)
        {
            var key = GetKey(expression);
            if (!mocks.ContainsKey(key))
            {
                return;
            }

            mocks[key]++;
        }

        public bool IsRegistered(Expression<Action<T>> expression) => this.mocks.ContainsKey(expression.Body.ToString());

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
            mocks.Add(key, 0);
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
    }
}
