using Stonylang_CSharp.Utility;
using Stonylang_CSharp.Evaluator;
using Stonylang_CSharp.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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

                var syntaxWatch = Stopwatch.StartNew();
                SyntaxTree.SyntaxTree syntaxTree = SyntaxTree.SyntaxTree.Parse(input);
                syntaxWatch.Stop();
                Console.WriteLine("SyntaxTree: " + syntaxWatch.ElapsedMilliseconds + "ms");
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
                        syntaxTree.Root.WriteTo(Console.Out);
                        Console.ResetColor();
                    }

                    Console.WriteLine(result.Value);
                }
            }
        }
    }
}
