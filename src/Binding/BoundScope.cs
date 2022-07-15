using Stonylang.Symbols;
using Stonylang.Utility;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stonylang.Binding
{
    internal sealed class BoundScope
    {
        private Dictionary<string, VariableSymbol> _variables;
        private Dictionary<string, FunctionSymbol> _functions;
        public BoundScope Parent { get; }

        public BoundScope(BoundScope parent) => Parent = parent;

        public bool TryLookUpVariable(string name, out VariableSymbol variable)
        {
            variable = null;
            if (_variables != null && _variables.TryGetValue(name, out variable))
                return true;
            if (Parent == null)
                return false;
            return Parent.TryLookUpVariable(name, out variable);
        }

        public bool TryDeclareVariable(VariableSymbol variable)
        {
            if (_variables == null)
                _variables = new();
            if (TryLookUpVariable(variable.Name, out _))
                return false;
            _variables.Add(variable.Name, variable);
            return true;
        }

        public bool TryLookUpFunction(string name, out FunctionSymbol function)
        {
            function = null;
            if (_functions != null && _functions.TryGetValue(name, out function))
                return true;
            if (Parent == null)
                return false;
            return Parent.TryLookUpFunction(name, out function);
        }

        public bool TryDeclareFunction(FunctionSymbol function)
        {
            if (_functions == null)
                _functions = new();

            if (TryLookUpFunction(function.Name, out _))
                return false;
            _functions.Add(function.Name, function);
            return true;
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables() => _variables == null ? ImmutableArray<VariableSymbol>.Empty : _variables.Values.ToImmutableArray();
        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() => _functions == null ? ImmutableArray<FunctionSymbol>.Empty : _functions.Values.ToImmutableArray();
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
