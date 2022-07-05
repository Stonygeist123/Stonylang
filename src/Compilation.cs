using Stonylang_CSharp.Binding;
using Stonylang_CSharp.Utility;
using System;
using System.Collections.Generic;
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
            Binder binder = new(_source, symbolTable);
            BoundExpr boundExpr = binder.BindExpr(Syntax.Root);

            DiagnosticBag diagnostics = Syntax.Diagnostics.AddRange(binder.Diagnostics);
            if (diagnostics.Any()) return new EvaluationResult(diagnostics, null);

            Evaluator evaluator = new(boundExpr, symbolTable);
            object value = evaluator.Evaluate();
            return new EvaluationResult(new(), value);
        }
    }
}
