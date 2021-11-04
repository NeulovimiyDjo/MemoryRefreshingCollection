using System;
using System.Collections.Generic;

namespace CalculatorLibrary
{
    public class Calculator
    {
        /// <summary>
        /// Calculates the result of the given expression if expression is valid or throws an exception if it isn't.
        /// </summary>
        ///<param name="expression">Expression to calculate.</param>
        public static double CalculateExpression(string expression)
        {
            var mainStack = Parser.ParseExpression(expression);
            var helpStack = new Stack<ExpressionElement>();

            ExpressionElement currElem, helpStackPeekResult;

            helpStack.Push(null);

            do
            {
                currElem = mainStack.Pop();

                if (currElem.Type == ElementTypes.Number)
                {
                    helpStackPeekResult = helpStack.Peek();

                    if (mainStack.Count == 0 && helpStackPeekResult == null)
                    {
                        return (currElem as Number).Value;
                    }            
                    else if (mainStack.Count == 0 || mainStack.Peek().Type == ElementTypes.Number)
                    {
                        if (mainStack.Count == 0 && (helpStackPeekResult.Type != ElementTypes.Operation || !(helpStackPeekResult as Operation).IsUnary))
                        {
                            throw new Exception("Algorithm mistake 3, this part of code shouldn't be ever reached");
                        }
                        else
                        {
                            PurshOperationResult(mainStack, helpStack, currElem);
                            PopFromHelp_PushToMain_WhileHelpElementIsNumber(mainStack, helpStack);
                        }
                    }
                    else if (mainStack.Peek().Type == ElementTypes.Operation)
                    {
                        helpStack.Push(currElem);
                    }
                }
                else if (currElem.Type == ElementTypes.Operation)
                {
                    helpStack.Push(currElem);
                }
            } while (true);

            throw new Exception("Algorithm mistake 1, this part of code shouldn't be ever reached");
        }

        /// <summary>
        /// Calculates the result of the given expression. If successful returns true, otherwise returns false.
        /// </summary>
        ///<param name="expression">Expression to calculate</param>
        ///<param name="result">The result of calculation if it was successful.</param>
        ///<example>lol</example>
        public static bool TryCalculateExpression(string expression, out double result)
        {
            result = 0;
            return true;
        }

        private static void PurshOperationResult(Stack<ExpressionElement> mainStack, Stack<ExpressionElement> helpStack, ExpressionElement currElem)
        {
            Operation lastOp = helpStack.Pop() as Operation;
            Number tempResult = null;
            if (!lastOp.IsUnary)
            {
                tempResult = mainStack.Pop() as Number;
            }
            double value;

            switch ((lastOp as Operation).Op)
            {
                case OperationTypes.Plus:
                    value = (tempResult as Number).Value + (currElem as Number).Value;
                    break;
                case OperationTypes.PlusUnary:
                    value = (currElem as Number).Value;
                    break;
                case OperationTypes.Minus:
                    value = (tempResult as Number).Value - (currElem as Number).Value;
                    break;
                case OperationTypes.MinusUnary:
                    value = -(currElem as Number).Value;
                    break;
                case OperationTypes.Multiply:
                    value = (tempResult as Number).Value * (currElem as Number).Value;
                    break;
                case OperationTypes.Divide:
                    value = (tempResult as Number).Value / (currElem as Number).Value;
                    break;
                case OperationTypes.Remainder:
                    value = (tempResult as Number).Value % (currElem as Number).Value;
                    break;
                case OperationTypes.Power:
                    value = Math.Pow((tempResult as Number).Value, (currElem as Number).Value);
                    break;
                case OperationTypes.Factorial:
                    value = Factorial((currElem as Number).Value);
                    break;
                case OperationTypes.Sin:
                    value = Math.Sin((currElem as Number).Value);
                    break;
                case OperationTypes.Cos:
                    value = Math.Cos((currElem as Number).Value);
                    break;
                default:
                    throw new Exception("Algorithm mistake 2, this part of code shouldn't be ever reached");
            }

            tempResult = new Number
            {
                Type = ElementTypes.Number,
                Value = value
            };

            mainStack.Push(tempResult);
        }

        private static void PopFromHelp_PushToMain_WhileHelpElementIsNumber(Stack<ExpressionElement> mainStack, Stack<ExpressionElement> helpStack)
        {
            ExpressionElement tempResult;

            while (helpStack.Peek()?.Type == ElementTypes.Number)
            {
                tempResult = helpStack.Pop();
                mainStack.Push(tempResult);
            }
        }

        private static double Factorial(double z)
        {
            if (z >= 0 && (z - Math.Round(z)) <= 0.001)
            {
                return Math.Round(MathNet.Numerics.SpecialFunctions.Gamma(z + 1));
            }
            else
            {
                return MathNet.Numerics.SpecialFunctions.Gamma(z + 1);
            }
        }
    }
}
