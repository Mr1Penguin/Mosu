using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace TenMock
{
    abstract public class Mock<T>
    {
        private Dictionary<string, object> mocks;

        public Mock<T> Register<TRes>(Expression<Func<T, TRes>> expression)
        {
            this.RegisterExpression(expression);
            return this;
        }

        public Mock<T> Register(Expression<Action<T>> expression)
        {
            this.RegisterExpression(expression);
            return this;
        }

        public bool IsRegistered(Expression<Action<T>> expression) => this.mocks.ContainsKey(expression.Body.ToString());

        public bool IsRegistered<TRes>(Expression<Func<T, TRes>> expression) => this.mocks.ContainsKey(expression.Body.ToString());

        private void RegisterExpression<TDeleg>(Expression<TDeleg> expression)
        {
            string key = GetKey(expression);
            mocks.Remove(key);
            mocks.Add(key, expression);
        }

        private static string GetKey<TDeleg>(Expression<TDeleg> expression) => expression.Body.ToString();
    }
}
