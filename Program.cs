using Stonylang_CSharp.Utility;
using Stonylang_CSharp.Evaluator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stonylang_CSharp
{
    internal static class Program
    {
        static readonly Dictionary<string, VariableSymbol> symbolTable = new();
        static bool showTree = false;
        static void Main()
        {
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
                Compilation compilation = new(syntaxTree, input);
                EvaluationResult result = compilation.Evaluate(symbolTable);

                DiagnosticBag diagnostics = result.Diagnostics;

                if (diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (Diagnostic diagnostic in diagnostics)
                        Console.WriteLine(diagnostic.Message + '\n');
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

                    Console.WriteLine(result.Value);
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
