using System;

namespace Stonylang_CSharp.Utility
{
    public struct TextSpan
    {
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
    }

    public enum LogLevel
    {
        Info, Warn, Error
    }

    public struct VariableSymbol
    {
        public VariableSymbol(string name, Type type, object value, TextSpan? span)
        {
            Name = name;
            Type = type;
            Value = value;
            Span = span;
        }

        public string Name { get; }
        public Type Type { get; }
        public object Value { get; set; }
        public TextSpan? Span { get; }
    }
}
