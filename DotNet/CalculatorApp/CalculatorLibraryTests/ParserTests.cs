using NUnit.Framework;
using System;
using System.Text;
using CalculatorLibrary;


namespace CalculatorLibraryTests
{
    [TestFixture]
    class ParserTests
    {
        // basic
        [TestCase("4", "4")]
        [TestCase("3+4", "3 4 +")]
        [TestCase("2+3*4*5-9", "2 3 4 * 5 * + 9 -")]
        [TestCase("4%3*2", "4 3 % 2 *")]
        // brackets and unary operations
        [TestCase("(3+4)", "3 4 +")]
        [TestCase("-3+4", "3 - 4 +")]      
        [TestCase("(-3+4)", "3 - 4 +")]
        [TestCase("(3)+(-4)", "3 4 - +")]
        [TestCase("(2+3)*4*(5-9)^5", "2 3 + 4 * 5 9 - 5 ^ *")]
        // special power priority
        [TestCase("2+3^2^4", "2 3 2 4 ^ ^ +")]
        // not an integer
        [TestCase("324.02^0.5+345*44.55", "324.02 0.5 ^ 345 44.55 * +")]
        // spaces
        [TestCase("   2 +  3   * 4*  5- 9 ", "2 3 4 * 5 * + 9 -")]
        // factorials
        [TestCase("4!", "4 !")]
        [TestCase("4-3!", "4 3 ! -")]
        [TestCase("(4+3)!", "4 3 + !")]
        [TestCase("4!*3", "4 ! 3 *")]
        [TestCase("4!^2", "4 ! 2 ^")]
        [TestCase("4!!", "4 ! !")]
        [TestCase("4^2!", "4 2 ! ^")]
        // functions
        [TestCase("sin(4)", "4 sin")]
        [TestCase("sIn(4)", "4 sin")]
        [TestCase("cos(4+3*2)", "4 3 2 * + cos")]
        [TestCase("cos(4)^2", "4 cos 2 ^")]
        [TestCase("sin(cos(4))", "4 cos sin")]
        [TestCase("sin((2))^(cos(1))", "2 sin 1 cos ^")]
        [TestCase("sin(cos(2342.324)^2.2)*sin(0.001)", "2342.324 cos 2.2 ^ sin 0.001 sin *")]
        public void ParseExpression_CorrectInput_CorrectResultReturned(string input, string expected)
        {
            var stack = Parser.ParseExpression(input);

            var sb = new StringBuilder("");          
            foreach (var element in stack)
            {
                sb.Insert(0, $"{element.ToString()} ");
            }
            string result = sb.ToString();
            result = result.TrimEnd();
            Assert.AreEqual(expected, result);
        }

        // empty string
        [TestCase("")]
        // starts with wrong symbol
        [TestCase("*4+2")]
        [TestCase(")2+2")]
        // ends with wrong symbol
        [TestCase("2+4-")]
        [TestCase("2+2(")]
        // bracket imbalance of amount
        [TestCase("3+(2+2))")]
        [TestCase("(3+(2+2)")]
        // bracket imbalance of order
        [TestCase("2)*(3+2")]
        // invalid previous symbol before number
        [TestCase("2+(4)2")]
        // invalid previous symbol before operation
        [TestCase("2++2")]
        [TestCase("2*(/2)")]
        // invalid previous symbol before opening bracket
        [TestCase("2+2(4)")]
        [TestCase("2+(3)(4)")]
        // invalid previous symbol before closing bracket
        [TestCase("2+(2*)-(4)")]
        [TestCase("2+()-(4)")]
        // dots in the wrong place
        [TestCase(".2")]
        [TestCase("2.")]
        [TestCase("2..4")]
        [TestCase("24.+2")]
        [TestCase("24+.2")]
        // factorials
        [TestCase("!")]
        [TestCase("2+(!2)")]
        [TestCase("2!3")]
        [TestCase("2!(4)")]
        [TestCase("2(!)")]
        [TestCase("2!(!)")]
        // invalid symbols
        [TestCase("23=47")]
        // functions
        [TestCase("sin2")]
        [TestCase("sin(s)")]
        public void ParseExpression_InvalidInput_Throw(string input)
        {
            var ex = Assert.Catch<Exception>(() => Parser.ParseExpression(input));

            StringAssert.Contains("Not a valid expression", ex.Message);
        }
    }
}
