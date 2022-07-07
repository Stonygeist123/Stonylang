using Stonylang_CSharp.Binding;
using Stonylang_CSharp.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Stonylang_CSharp.Evaluator
{
    public class Compilation
    {
        private BoundGlobalScope _globalScope = null;
        private readonly SourceText _source;
        public SyntaxTree.SyntaxTree Syntax { get; }
        public Compilation Previous { get; }
        private Compilation(Compilation previous, SyntaxTree.SyntaxTree syntax, SourceText source)
        {
            Previous = previous;
            Syntax = syntax;
            _source = source;
        }

        public Compilation(SyntaxTree.SyntaxTree syntax, SourceText source) : this(null, syntax, source)
        {
            Syntax = syntax;
            _source = source;
        }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    _globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, _source, Syntax.Root);
                    Interlocked.CompareExchange(ref _globalScope, _globalScope, null);
                }
                return _globalScope;
            }
        }

        public Compilation ContinueWith(SyntaxTree.SyntaxTree syntax, SourceText source) => new(this, syntax, source);

        public EvaluationResult Evaluate(Dictionary<string, VariableSymbol> symbolTable)
        {
            var binderWatch = Stopwatch.StartNew();
            BoundStmt boundStmt = GlobalScope.Statement;
            binderWatch.Stop();
            Console.WriteLine("TypeChecking: " + binderWatch.ElapsedMilliseconds + "ms");

            DiagnosticBag diagnostics = Syntax.Diagnostics.AddRange(GlobalScope.Diagnostics);
            if (diagnostics.Any()) return new EvaluationResult(diagnostics, null);

            var evaluationWatch = Stopwatch.StartNew();
            Evaluator evaluator = new(boundStmt, symbolTable);
            object value = evaluator.Evaluate();
            evaluationWatch.Stop();
            Console.WriteLine("Evaluating: " + evaluationWatch.ElapsedMilliseconds + "ms\n");
            return new EvaluationResult(new(), value);
        }
    }
}
