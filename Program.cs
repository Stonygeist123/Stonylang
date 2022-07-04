using Stonylang_CSharp.Binding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stonylang_CSharp
{
    internal static class Program
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
                else if (input.ToLower() == "#cls")
                {
                    Console.Clear();
                    continue;
                }

                SyntaxTree.SyntaxTree syntaxTree = SyntaxTree.SyntaxTree.Parse(input);
                Binder binder = new();
                BoundExpr boundExpr = binder.BindExpr(syntaxTree.Root);

                string[] diagnostics = syntaxTree.Diagnostics.Concat(binder.Diagnostics).ToArray();

                if (diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (string diagnostic in diagnostics)
                        Console.WriteLine(diagnostic);
                    Console.ResetColor();
                }
                else
                {
                    if (showTree)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        PrettyPrint(syntaxTree.Root);
                        Console.ResetColor();
                    }

                    Evaluator.Evaluator evaluator = new(boundExpr);
                    object result = evaluator.Evaluate();
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

            indent += isLast ? "   " : "|   ";
            Parser.INode lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren()) PrettyPrint(child, indent, child == lastChild);
        }
    }
}
