using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Mosu
{
    internal class KeyGenerator
    {
        internal static string GetKey<TDeleg>(Expression<TDeleg> expression)
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
    }
}
