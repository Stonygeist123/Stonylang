using Stonylang.Binding;
using Stonylang.Lowering;
using Stonylang.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Stonylang.Evaluator
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
            DiagnosticBag diagnostics = Syntax.Diagnostics.AddRange(GlobalScope.Diagnostics);
            if (diagnostics.Any()) return new EvaluationResult(diagnostics, null);

            BoundBlockStmt stmt = GetStatement();
            Evaluator evaluator = new(stmt, symbolTable);
            object value = evaluator.Evaluate();
            return new EvaluationResult(new(), value);
        }

        public void EmitTree(TextWriter writer)
        {
            BoundStmt stmt = GetStatement();
            stmt.WriteTo(writer);
        }

        private BoundBlockStmt GetStatement() => Lowerer.Lower(GlobalScope.Statement);
    }
}
