using Stonylang.Utility;
using System;

namespace Stonylang.Symbols
{
    public enum SymbolKind
    {
        Type,
        Variable
    }

    public abstract class Symbol
    {
        private protected Symbol(string name) => Name = name;
        public string Name { get; }
        public abstract SymbolKind Kind { get; }
        public override string ToString() => Name;
    }

    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new("unknown");
        public static readonly TypeSymbol Bool = new("bool");
        public static readonly TypeSymbol Int = new("int");
        public static readonly TypeSymbol String = new("string");
        internal TypeSymbol(string name)
            : base(name) { }

        public override SymbolKind Kind => SymbolKind.Type;
    }

    public sealed class VariableSymbol : Symbol
    {
        public VariableSymbol(string name, TypeSymbol type, object value, TextSpan? span, bool isMut = false)
            : base(name)
        {
            Type = type;
            Value = value;
            Span = span;
            IsMut = isMut;
        }

        public TypeSymbol Type { get; }
        public object Value { get; set; }
        public TextSpan? Span { get; }
        public bool IsMut { get; }
        public override SymbolKind Kind => SymbolKind.Variable;
    }
}
