using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using GrammarConverter.Generated;

namespace GrammarConverter
{
    public class Program
    {
        public static void Main()
        {
            string inputFilePath = @"../../../Grammars/PGGrammarInput.txt";
            string outputFilePath = @"../../../Grammars/PGGrammarOutput.txt";
            string oldGrammar = File.ReadAllText(inputFilePath);

            AntlrInputStream inputStream = new(oldGrammar);
            ANTLRGrammarLexer lexer = new(inputStream);
            CommonTokenStream tokenStream = new(lexer);
            ANTLRGrammarParser parser = new(tokenStream);

            ANTLRGrammarParser.RootContext rootContext = parser.root();
            GetConvertedANTLRGrammarVisitor visitor = new();
            string newGrammar = rootContext.Accept(visitor);
            File.WriteAllText(outputFilePath, newGrammar);
        }
    }

    public class GetConvertedANTLRGrammarVisitor : ANTLRGrammarParserBaseVisitor<string>
    {
        private readonly StringBuilder _sb = new();
        private bool _firstRun;
        private bool _firstChild;
        private readonly HashSet<string> _initialRuleNames = new();

        public override string VisitRoot([NotNull] ANTLRGrammarParser.RootContext context)
        {
            _initialRuleNames.Clear();
            _firstRun = true;
            VisitChildren(context);

            _sb.Clear();
            _firstRun = false;
            VisitChildren(context);

            return _sb.ToString();
        }

        public override string VisitRule_spec([NotNull] ANTLRGrammarParser.Rule_specContext context)
        {
            string ruleName = context.rule_name().IDENTIFIER().ToString();
            if (_firstRun)
            {
                _initialRuleNames.Add(ruleName);
            }
            else
            {
                ruleName = ruleName.ToLower();
                _sb.Append(ruleName).Append("\n");

                _firstChild = true;
                VisitChildren(context);

                _sb.Append("   ;\n\n");
            }
            return null;
        }

        public override string VisitRule_val([NotNull] ANTLRGrammarParser.Rule_valContext context)
        {
            if (_firstChild)
                _sb.Append("   :");
            else
                _sb.Append("   |");
            _firstChild = false;

            VisitChildren(context);
            _sb.Append("\n");

            return null;
        }

        public override string VisitVal_item([NotNull] ANTLRGrammarParser.Val_itemContext context)
        {
            string identifier = context.IDENTIFIER()?.ToString();
            if (_initialRuleNames.Contains(identifier))
                identifier = identifier.ToLower();
            if (identifier is not null)
                _sb.Append(" ").Append(identifier);

            string squotaString = context.SQUOTA_STRING()?.ToString();
            if (squotaString is not null)
                _sb.Append(" ").Append(squotaString);

            return null;
        }
    }
}
