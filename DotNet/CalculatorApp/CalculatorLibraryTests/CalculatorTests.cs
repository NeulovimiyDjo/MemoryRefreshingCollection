using NUnit.Framework;
using System;
using CalculatorLibrary;


namespace CalculatorLibraryTests
{
    [TestFixture]
    class CalculatorTests
    {
        // basic
        [TestCase("4", 4)]          // 4
        [TestCase("3+4", 7)]        // 3 4 +
        [TestCase("2+3*4*5-9", 53)] // 2 3 4 * 5 * + 9 -
        [TestCase("3+4*2^3", 35)]   // 2 4 2 3 ^ * +
        [TestCase("4%3*5", 5)]    // 4 3 % 5 *
        // not an integer
        [TestCase("324.02^0.5+345*44.55", 15387.7505555)] // 324.02 0.5 ^ 345 44.55 * +
        // division by zero and complex numbers
        [TestCase("4+2/(3-3)", Double.PositiveInfinity)]   // 2 4 2 3 ^ * +
        [TestCase("4-2/(3-3)", Double.NegativeInfinity)]   // 2 4 2 3 ^ * -
        [TestCase("(1-3)^(1/2)", Double.NaN)]  // 1 3 - 1 2 / ^
        // unary plus and minus
        [TestCase("-2", -2)]   // 2 -u
        [TestCase("+2-(-2)", 4)]   // 2 +u 2 -u -
        [TestCase("2-(-2*4+10)^3", -6)]   // 2 2 -u 4 * 10 + 3 ^ -
        // factorials
        [TestCase("4!", 24)]    // 4 !
        [TestCase("4!*3", 72)]  // 4 ! 3 *
        [TestCase("3!!", 720)]  // 3 ! !
        [TestCase("3.3!", 8.85534336045)]  // 3.3 !
        // aka gamma function from arguments < 1
        [TestCase("(-22.2)!", 0.00000000001)]
        [TestCase("0!", 0.99999999999)]
        [TestCase("(-1)!", Double.PositiveInfinity)]
        // functions
        [TestCase("sin(4)", -0.7568024953)]   // 4 sin
        [TestCase("cOs(4+3*2)", -0.83907152907)]   // 4 3 2 * + cos
        [TestCase("cos(4)^2", 0.42724998309)]     // 4 cos 2 ^
        [TestCase("sin(cos(4))", -0.60808300964)]  // 4 cos sin
        [TestCase("sin(cos(2342.324)^2.2)*sin(0.001)", 0.00005316316)]   // 2342.324 cos 2.2 ^ sin 0.001 sin *
        public void CalculateExpression_CorrectInput_CorrectResultReturned(string input, double expected)
        {
            double result = Calculator.CalculateExpression(input);

            Assert.AreEqual(expected, result, 0.001);
        }

        // invalid string
        [TestCase("2+()")]
        public void CalculateExpression_InvalidInput_Throw(string input)
        {
            var ex = Assert.Catch<Exception>(() => Calculator.CalculateExpression(input));

            StringAssert.Contains("Not a valid expression", ex.Message);
        }
    }
}
