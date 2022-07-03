using System;
using System.Linq;

namespace Stonylang_CSharp
{
    class Program
    {
        static void Main()
        {
            bool showTree = false;
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.ToLower() == "#showtree")
                {
                    showTree ^= true;
                    Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees.");
                    continue;
                }
                else if (input.ToLower() == "#cls") Console.Clear();

                SyntaxTree.SyntaxTree syntaxTree = SyntaxTree.SyntaxTree.Parse(input);

                var color = Console.ForegroundColor;
                if (syntaxTree.Diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (string diagnostic in syntaxTree.Diagnostics)
                        Console.WriteLine(diagnostic);
                    Console.ForegroundColor = color;
                }
                else
                {
                    if (showTree)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        PrettyPrint(syntaxTree.Root);
                        Console.ForegroundColor = color;
                    }

                    Evaluator.Evaluator evaluator = new(syntaxTree.Root);
                    int result = evaluator.Evaluate();
                    Console.WriteLine(result);
                }
            }
        }
        // └──
        // ├──
        // |
        static void PrettyPrint(Parser.INode node, string indent = "", bool isLast = true)
        {
            string marker = isLast ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);

            if (node is Lexer.Token @t && t.Literal != null)
            {
                Console.Write(" ");
                Console.Write(t.Literal);
            }
            Console.WriteLine();

            indent += isLast ? "    " : "|   ";
            Parser.INode last = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren()) PrettyPrint(child, indent, child == last);
        }
    }
}
