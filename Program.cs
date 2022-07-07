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
        static void Main()
        {
            Dictionary<string, VariableSymbol> symbolTable = new();
            bool showTree = false;
            Compilation previous = null;

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input.Replace('\r', '\0').Replace('\n', '\0'))) continue;
                SourceText source = SourceText.From(input);

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
                else if (input.ToLower() == "#reset")
                {
                    previous = null;
                    continue;
                }

                var syntaxWatch = Stopwatch.StartNew();
                SyntaxTree.SyntaxTree syntaxTree = SyntaxTree.SyntaxTree.Parse(input);
                syntaxWatch.Stop();
                Console.WriteLine("SyntaxTree: " + syntaxWatch.ElapsedMilliseconds + "ms");
                Compilation compilation = previous == null
                    ? new(syntaxTree, source) : previous.ContinueWith(syntaxTree, source);
                EvaluationResult result = compilation.Evaluate(symbolTable);
                DiagnosticBag diagnostics = result.Diagnostics;

                if (diagnostics.Any())
                    foreach (Diagnostic diagnostic in diagnostics)
                        diagnostic.Print();
                else
                {
                    previous = compilation;
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
