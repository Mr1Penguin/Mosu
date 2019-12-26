using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            AddArguments(sb, body.Arguments, method);
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

        private static void AddArguments(
            StringBuilder sb, 
            IReadOnlyCollection<Expression> Arguments,
            MethodInfo methodInfo)
        {
            sb.Append("(");
            var parameters = methodInfo.GetParameters();
            foreach (var param in Arguments.Zip(methodInfo.GetParameters(), (arg, par) => (arg, par)))
            {
                switch (param.arg)
                {
                    case ConstantExpression e:
                        sb.Append(e.Type.Name);
                        break;
                    case MethodCallExpression e:
                        if (e.Method.DeclaringType.Name == nameof(Arg))
                        {
                            sb.Append(param.par.ParameterType.Name);
                        }
                        else
                        {
                            sb.Append(e.Method.ReturnType.Name);
                        }
                        break;
                    default:
                        sb.Append(param.arg.Type.Name);
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
