using Stonylang.Symbols;
using Stonylang.Utility;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stonylang.Binding
{

    internal sealed class BoundScope
    {
        private readonly Dictionary<string, VariableSymbol> _variables = new();
        public BoundScope Parent { get; }
        public BoundScope(BoundScope parent) => Parent = parent;

        public ImmutableArray<VariableSymbol> GetDeclaredVariables() => _variables.Values.ToImmutableArray();
        public bool TryLookUp(string name, out VariableSymbol variable) => _variables.TryGetValue(name, out variable) || (Parent != null && Parent.TryLookUp(name, out variable));
        public bool TryDeclare(VariableSymbol variable, out VariableSymbol oldVariable)
        {
            if (TryLookUp(variable.Name, out var v))
            {
                oldVariable = v;
                return false;
            }
            oldVariable = variable;
            _variables.Add(variable.Name, variable);
            return true;
        }
    }

    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(BoundGlobalScope previous, DiagnosticBag diagnostics, ImmutableArray<VariableSymbol> variables, BoundStmt statement)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            Variables = variables;
            Statement = statement;
        }

        public BoundGlobalScope Previous { get; }
        public DiagnosticBag Diagnostics { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public BoundStmt Statement { get; }
    }
}
