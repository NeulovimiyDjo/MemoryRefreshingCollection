using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace MyProjectTests
{
    public class TestCasesCountInNamespaceBasedOrderAttribute : OrderAttribute
    {
        public TestCasesCountInNamespaceBasedOrderAttribute(Type typeDefiningNamespaceToLookIn)
            : base(Priority(typeDefiningNamespaceToLookIn)) { }

        private static int Priority(Type typeDefiningNamespaceToLookIn)
        {
            var types = Assembly.GetAssembly(typeDefiningNamespaceToLookIn)
                .GetTypes()
                .Where(x => x.IsClass
                    && x.Namespace?.StartsWith(typeDefiningNamespaceToLookIn.Namespace) == true)
                .ToList();

            int count = types
                .Select(x => x
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.IsDefined(typeof(TestAttribute), true))
                    .Count())
                .Aggregate((x, y) => x + y);

            return count == 0 ? int.MaxValue : int.MaxValue / count;
        }
    }
}
