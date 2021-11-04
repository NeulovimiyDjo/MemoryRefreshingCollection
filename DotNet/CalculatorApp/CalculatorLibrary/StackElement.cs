using System.Globalization;

namespace CalculatorLibrary
{
    internal enum ElementTypes
    {
        Number,
        Operation,
        Bracket
    }

    internal enum OperationTypes
    {
        Plus,
        PlusUnary,
        Minus,
        MinusUnary,
        Multiply,
        Divide,
        Remainder,
        Power,
        Factorial,
        Sin,
        Cos
    }

    internal enum OperationPriorities
    {
        PlusMinus = 1,
        PlusMinusUnary = 2,
        MultiplyDivide = 2,
        Remainder = 2,
        Power = 3,
        Factorial = 4,
        Function = 5
    }

    internal enum BracketTypes
    {
        Opening,
        Closing
    }

    internal class ExpressionElement
    {
        internal ElementTypes Type { get; set; }

        internal int StringLength { get; set; }
    }

    internal class Number : ExpressionElement
    {
        internal double Value { get; set; }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }

    internal class Bracket : ExpressionElement
    {
        internal BracketTypes BracketType { get; set; }

        public override string ToString()
        {
            switch (BracketType)
            {
                case BracketTypes.Opening:
                    return "(";
                case BracketTypes.Closing:
                    return ")";
                default:
                    return "BracketBracketTypeNotSet";
            }
        }
    }

    internal class Operation : ExpressionElement
    {
        internal OperationTypes Op { get; set; }

        internal OperationPriorities Priority { get; set; }

        internal bool IsUnary { get; set; } = false;

        internal bool IsFunction { get; set; } = false;

        public override string ToString()
        {
            switch (Op)
            {
                case OperationTypes.Plus:
                    return "+";
                case OperationTypes.Minus:
                    return "-";
                case OperationTypes.PlusUnary:
                    return "+";
                case OperationTypes.MinusUnary:
                    return "-";
                case OperationTypes.Multiply:
                    return "*";
                case OperationTypes.Divide:
                    return "/";
                case OperationTypes.Remainder:
                    return "%";
                case OperationTypes.Power:
                    return "^";
                case OperationTypes.Factorial:
                    return "!";
                case OperationTypes.Sin:
                    return "sin";
                case OperationTypes.Cos:
                    return "cos";
                default:
                    return "OperationValueNotSet";
            }
        }
    }
}
