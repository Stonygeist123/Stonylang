using Stonylang_CSharp.Binding;
using Stonylang_CSharp.Diagnostics;
using System;
using System.Linq;

namespace Stonylang_CSharp.Evaluator
{
    public class Compilation
    {
        private readonly string _source;
        public Compilation(SyntaxTree.SyntaxTree syntax, string source) { Syntax = syntax; _source = source; }
        public SyntaxTree.SyntaxTree Syntax { get; }

        public EvaluationResult Evaluate()
        {
            Binder binder = new(_source);
            BoundExpr boundExpr = binder.BindExpr(Syntax.Root);

            DiagnosticBag diagnostics = Syntax.Diagnostics.AddRange(binder.Diagnostics);
            if (diagnostics.Any()) return new EvaluationResult(diagnostics, null);

            Evaluator evaluator = new(boundExpr);
            object value = evaluator.Evaluate();
            return new EvaluationResult(new(), value);
        }
    }
}
