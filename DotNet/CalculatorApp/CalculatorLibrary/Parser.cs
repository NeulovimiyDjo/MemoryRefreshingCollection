using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CalculatorLibrary
{
    internal class Parser
    {
        /// <summary>
        /// Parses the given expression and returns the result in Reverse Polish Notation form if expression is valid or throws an exception if it isn't.
        /// </summary>
        ///<param name="expression">Expression to parse.</param>
        internal static Stack<ExpressionElement> ParseExpression(string expression)
        {
            if (!DoBasicValidation(ref expression))
            {
                throw new Exception("Not a valid expression");
            }

            var mainStack = new Stack<ExpressionElement>();
            var helpStack = new Stack<ExpressionElement>();
            ExpressionElement currElem;
            ExpressionElement prevElem = null;

            helpStack.Push(null);

            while (expression.Length > 0)
            {
                currElem = GetFirstExpressionElement(expression, prevElem);

                if (currElem.Type == ElementTypes.Number)
                {
                    TryHandle_Number(mainStack, currElem, prevElem);
                }
                else if (currElem.Type == ElementTypes.Bracket)
                {
                    TryHandle_Bracket(mainStack, helpStack, currElem, prevElem);
                }
                else if (currElem.Type == ElementTypes.Operation)
                {
                    TryHandle_Operation(mainStack, helpStack, currElem, prevElem);
                }

                prevElem = currElem;
                expression = expression.Remove(0, currElem.StringLength);
            }

            PopFromHelp_PushToMain_UntilHelpEnds(mainStack, helpStack);

            return mainStack;
        }

 
        /// <summary>
        /// Tries to parse the expression into Reverse Polish Notation form. If successful returns true, otherwise returns false.
        /// </summary>
        ///<param name="expression">Expression to parse.</param>
        ///<param name="stack">Stack to keep the result in Reverse Polish Notation form</param>
        internal static bool TryParseExpression(string expression, Stack<ExpressionElement> stack)
        {
            return true;
        }

        private static bool DoBasicValidation(ref string expression)
        {
            bool result = true;

            // Clear white space
            Regex regex = new Regex(@"(\s+)");
            expression = regex.Replace(expression, "");

            // Tolower for function parsing
            expression = expression.ToLowerInvariant();

            // Validate allowed symbols and check for empty string
            regex = new Regex(@"^[\d|\.|\+|\-|\*|\/|\%|\^|\(|\)|\!|s|i|n|c|o]*$");
            if (expression.Length == 0 || !regex.IsMatch(expression))
            {
                result = false;
            }

            // Validate allowed first symbol
            regex = new Regex(@"^[\d|\+|\-|\(|s|i|n|c|o]");
            if (!regex.IsMatch(expression))
            {
                result = false;
            }

            // Validate allowed last symbol
            regex = new Regex(@"[\d|\)|\!]$");
            if (!regex.IsMatch(expression))
            {
                result = false;
            }

            // Validate bracket amount balance
            regex = new Regex(@"(\()");
            MatchCollection openingBrackets = regex.Matches(expression);
            regex = new Regex(@"(\))");
            MatchCollection closingBrackets = regex.Matches(expression);
            if (openingBrackets.Count != closingBrackets.Count)
            {
                result = false;
            }

            return result;
        }

        private static ExpressionElement GetFirstExpressionElement(string expression, ExpressionElement prevElem)
        {
            switch (expression[0])
            {
                case '+':
                    if (prevElem == null || prevElem.Type == ElementTypes.Bracket && (prevElem as Bracket).BracketType == BracketTypes.Opening)
                    {
                        return new Operation
                        {
                            Type = ElementTypes.Operation,
                            StringLength = 1,
                            Op = OperationTypes.PlusUnary,
                            Priority = OperationPriorities.PlusMinusUnary,
                            IsUnary = true
                        };
                    }
                    else
                    {
                        return new Operation
                        {
                            Type = ElementTypes.Operation,
                            StringLength = 1,
                            Op = OperationTypes.Plus,
                            Priority = OperationPriorities.PlusMinus,
                        };
                    }                   
                case '-':
                    if (prevElem == null || prevElem.Type == ElementTypes.Bracket && (prevElem as Bracket).BracketType == BracketTypes.Opening)
                    {
                        return new Operation
                        {
                            Type = ElementTypes.Operation,
                            StringLength = 1,
                            Op = OperationTypes.MinusUnary,
                            Priority = OperationPriorities.PlusMinusUnary,
                            IsUnary = true
                        };
                    }
                    else
                    {
                        return new Operation
                        {
                            Type = ElementTypes.Operation,
                            StringLength = 1,
                            Op = OperationTypes.Minus,
                            Priority = OperationPriorities.PlusMinus
                        };
                    }
                case '*':
                    return new Operation
                    {
                        Type = ElementTypes.Operation,
                        StringLength = 1,
                        Op = OperationTypes.Multiply,
                        Priority = OperationPriorities.MultiplyDivide
                    };
                case '/':
                    return new Operation
                    {
                        Type = ElementTypes.Operation,
                        StringLength = 1,
                        Op = OperationTypes.Divide,
                        Priority = OperationPriorities.MultiplyDivide
                    };
                case '%':
                    return new Operation
                    {
                        Type = ElementTypes.Operation,
                        StringLength = 1,
                        Op = OperationTypes.Remainder,
                        Priority = OperationPriorities.Remainder
                    };
                case '^':
                    return new Operation
                    {
                        Type = ElementTypes.Operation,
                        StringLength = 1,
                        Op = OperationTypes.Power,
                        Priority = OperationPriorities.Power
                    };
                case '!':
                    return new Operation
                    {
                        Type = ElementTypes.Operation,
                        StringLength = 1,
                        Op = OperationTypes.Factorial,
                        Priority = OperationPriorities.Factorial,
                        IsUnary = true
                    };
                case '(':
                    return new Bracket
                    {
                        Type = ElementTypes.Bracket,
                        StringLength = 1,
                        BracketType = BracketTypes.Opening
                    };
                case ')':
                    return new Bracket
                    {
                        Type = ElementTypes.Bracket,
                        StringLength = 1,
                        BracketType = BracketTypes.Closing
                    };
                case '.':
                    throw new Exception("Not a valid expression");  // dot can only be in a middle of a number
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    string numberAsString = ReturnFirstNumber(expression);

                    double value = Double.Parse(numberAsString, CultureInfo.InvariantCulture);

                    return new Number
                    {
                        Type = ElementTypes.Number,
                        StringLength = numberAsString.Length,
                        Value = value
                    };
                default:
                    return ReturnFirstFunction(expression);
            }

        }

        private static string ReturnFirstNumber(string expression)
        {
            Regex regex = new Regex(@"^(\d+\.\d+|^\d+)");
            Match match = regex.Match(expression);
            return match.Value;
        }

        private static Operation ReturnFirstFunction(string expression)
        {
            Regex regex = new Regex(@"^(sin)(\()");
            if (regex.IsMatch(expression))
            {
                return new Operation
                {
                    Type = ElementTypes.Operation,
                    StringLength = 3,
                    Op = OperationTypes.Sin,
                    Priority = OperationPriorities.Function,
                    IsUnary = true,
                    IsFunction = true
                };
            }

            regex = new Regex(@"^(cos)(\()");
            if (regex.IsMatch(expression))
            {
                return new Operation
                {
                    Type = ElementTypes.Operation,
                    StringLength = 3,
                    Op = OperationTypes.Cos,
                    Priority = OperationPriorities.Function,
                    IsUnary = true,
                    IsFunction = true
                };
            }

            throw new Exception("Not a valid expression");
        }

        private static void TryHandle_Number(Stack<ExpressionElement> mainStack, ExpressionElement currElem, ExpressionElement prevElem)
        {
            if (prevElem == null || prevElem.Type == ElementTypes.Operation && (prevElem as Operation).Op != OperationTypes.Factorial ||
                prevElem.Type == ElementTypes.Bracket && (prevElem as Bracket).BracketType == BracketTypes.Opening)
            {
                mainStack.Push(currElem);
            }
            else
            {
                throw new Exception("Not a valid expression");
            }
        }

        private static void TryHandle_Bracket(Stack<ExpressionElement> mainStack, Stack<ExpressionElement> helpStack,
                                                ExpressionElement currElem, ExpressionElement prevElem)
        {
            ExpressionElement peekResult, tempResult;

            if ((currElem as Bracket).BracketType == BracketTypes.Opening)
            {
                if (prevElem == null || prevElem.Type == ElementTypes.Operation && (prevElem as Operation).Op != OperationTypes.Factorial ||
                    prevElem.Type == ElementTypes.Bracket && (prevElem as Bracket).BracketType == BracketTypes.Opening)
                {
                    helpStack.Push(currElem);
                }
                else
                {
                    throw new Exception("Not a valid expression");
                }
            }

            else if ((currElem as Bracket).BracketType == BracketTypes.Closing)
            {
                if (prevElem?.Type == ElementTypes.Number || prevElem?.Type == ElementTypes.Operation && (prevElem as Operation).Op == OperationTypes.Factorial ||
                    prevElem?.Type == ElementTypes.Bracket && (prevElem as Bracket).BracketType == BracketTypes.Closing)
                {
                    peekResult = helpStack.Peek();
                    while (peekResult?.Type == ElementTypes.Operation)
                    {
                        tempResult = helpStack.Pop();
                        mainStack.Push(tempResult);
                        peekResult = helpStack.Peek();
                    }
                    if ((peekResult as Bracket)?.BracketType == BracketTypes.Opening)
                    {
                        helpStack.Pop();
                    }
                    else
                    {
                        throw new Exception("Not a valid expression");  // Didnt find corresponding opening bracket
                    }
                }
                else
                {
                    throw new Exception("Not a valid expression");
                }
            }
        }

        private static void TryHandle_Operation(Stack<ExpressionElement> mainStack, Stack<ExpressionElement> helpStack,
                                                ExpressionElement currElem, ExpressionElement prevElem)
        {
            ExpressionElement peekResult, tempResult;

            if (prevElem == null || prevElem.Type == ElementTypes.Number || prevElem.Type == ElementTypes.Operation &&
                ((currElem as Operation).IsFunction || (prevElem as Operation).Op == OperationTypes.Factorial) ||
                prevElem.Type == ElementTypes.Bracket && ((prevElem as Bracket).BracketType == BracketTypes.Closing || (currElem as Operation).IsUnary))
            {
                peekResult = helpStack.Peek();
                while (peekResult?.Type != ElementTypes.Bracket && ((currElem as Operation).Priority < (peekResult as Operation)?.Priority ||
                        (currElem as Operation).Priority == (peekResult as Operation)?.Priority && (currElem as Operation).Priority != OperationPriorities.Power))
                {
                    tempResult = helpStack.Pop();
                    mainStack.Push(tempResult);
                    peekResult = helpStack.Peek();
                }
                helpStack.Push(currElem);
            }
            else
            {
                throw new Exception("Not a valid expression");
            }
        }

        private static void PopFromHelp_PushToMain_UntilHelpEnds(Stack<ExpressionElement> mainStack, Stack<ExpressionElement> helpStack)
        {
            ExpressionElement tempResult;

            while (helpStack.Peek() != null)
            {
                tempResult = helpStack.Pop();
                if (!(tempResult.Type == ElementTypes.Bracket && (tempResult as Bracket).BracketType == BracketTypes.Opening))
                {
                    mainStack.Push(tempResult);
                }
            }
        }
    }
}
