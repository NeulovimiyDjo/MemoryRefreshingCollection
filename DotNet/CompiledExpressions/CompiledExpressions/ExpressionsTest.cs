using System;
using System.Linq.Expressions;

namespace CompiledExpressions
{
    public static class ExpressionsTest
    {
        public static void Run()
        {
            var obj = "sdsd";

            var paramExpr1 = Expression.Parameter(typeof(char));
            var paramExpr2 = Expression.Parameter(typeof(char));

            var bodyExpr = Expression.Call(
                Expression.Constant(obj),
                "Replace",
                null,
                paramExpr1,
                paramExpr2
            );

            var funcExpr = Expression.Lambda<Func<char, char, string>>(
                bodyExpr,
                paramExpr1,
                paramExpr2
            );

            var func = funcExpr.Compile();

            char param1 = 's';
            char param2 = 'x';
            var res = func(param1, param2);

            Console.WriteLine(res);





            ParameterExpression parameterX = Expression.Parameter(typeof(int), "x");
            ParameterExpression parameterY = Expression.Parameter(typeof(int), "y");
            ParameterExpression parameterZ = Expression.Parameter(typeof(int), "z");
            Expression multiplyYZ = Expression.Multiply(parameterY, parameterZ);
            Expression addXMultiplyYZ = Expression.Add(parameterX, multiplyYZ);
            var fe = Expression.Lambda<Func<int, int, int, int>>
            (
                addXMultiplyYZ,
                parameterX,
                parameterY,
                parameterZ
            );
            var f = fe.Compile();
            Console.WriteLine(f(24, 6, 3)); // prints 42 to the console
        }
    }
}
