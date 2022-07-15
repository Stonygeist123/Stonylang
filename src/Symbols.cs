using Stonylang.Utility;
using System.Collections.Immutable;

namespace Stonylang.Symbols
{
    public enum SymbolKind
    {
        Type,
        Variable,
        Function,
        Parameter
    }

    public abstract class Symbol
    {
        protected Symbol(string name) => Name = name;

        public string Name { get; }
        public abstract SymbolKind Kind { get; }

        public override string ToString() => Name;
    }

    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new("unknown");
        public static readonly TypeSymbol Any = new("any");
        public static readonly TypeSymbol Bool = new("bool");
        public static readonly TypeSymbol Int = new("int");
        public static readonly TypeSymbol Float = new("float");
        public static readonly TypeSymbol String = new("string");
        public static readonly TypeSymbol Void = new("void");

        internal TypeSymbol(string name)
            : base(name) { }

        public override SymbolKind Kind => SymbolKind.Type;
    }

    public class VariableSymbol : Symbol
    {
        public VariableSymbol(string name, TypeSymbol type, object value, TextSpan? span, bool isMut = false)
            : base(name)
        {
            Type = type;
            Value = value;
            Span = span;
            IsMut = isMut;
        }

        public override SymbolKind Kind => SymbolKind.Variable;
        public TypeSymbol Type { get; }
        public object Value { get; set; }
        public TextSpan? Span { get; }
        public bool IsMut { get; }
    }

    public sealed class ParameterSymbol : VariableSymbol
    {
        public ParameterSymbol(string name, TypeSymbol type, TextSpan? span, bool isMut = false)
            : base(name, type, null, span, isMut)
        {
        }

        public override SymbolKind Kind => SymbolKind.Parameter;
    }

    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type, TextSpan? span)
            : base(name)
        {
            Parameters = parameters;
            Type = type;
            Span = span;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol Type { get; }
        public TextSpan? Span { get; }
    }
}
