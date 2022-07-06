using Stonylang_CSharp.Binding;
using Stonylang_CSharp.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Stonylang_CSharp.Evaluator
{
    public class Compilation
    {
        private readonly string _source;
        public Compilation(SyntaxTree.SyntaxTree syntax, string source) { Syntax = syntax; _source = source; }
        public SyntaxTree.SyntaxTree Syntax { get; }

        public EvaluationResult Evaluate(Dictionary<string, VariableSymbol> symbolTable)
        {
            var binderWatch = Stopwatch.StartNew();
            Binder binder = new(SourceText.From(_source), symbolTable);
            BoundExpr boundExpr = binder.BindExpr(Syntax.Root);
            binderWatch.Stop();
            Console.WriteLine("TypeChecking: " + binderWatch.ElapsedMilliseconds + "ms");

            DiagnosticBag diagnostics = Syntax.Diagnostics.AddRange(binder.Diagnostics);
            if (diagnostics.Any()) return new EvaluationResult(diagnostics, null);

            var evaluationWatch = Stopwatch.StartNew();
            Evaluator evaluator = new(boundExpr, symbolTable);
            object value = evaluator.Evaluate();
            evaluationWatch.Stop();
            Console.WriteLine("Evaluating: " + evaluationWatch.ElapsedMilliseconds + "ms\n");
            return new EvaluationResult(new(), value);
        }
    }
}
