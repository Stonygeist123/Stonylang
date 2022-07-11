using Stonylang_CSharp.Binding;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stonylang_CSharp.Utility
{
    public enum LogLevel
    {
        Info, Warn, Error
    }

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
        public static TextSpan FromRounds(int start, int end) => new(start, end - start);
        public override string ToString() => $"{Start}..{End}";
    }

    public struct VariableSymbol
    {
        public VariableSymbol(string name, Type type, object value, TextSpan? span, bool isMut = false)
        {
            Name = name;
            Type = type;
            Value = value;
            Span = span;
            IsMut = isMut;
        }

        public string Name { get; }
        public Type Type { get; }
        public object Value { get; set; }
        public TextSpan? Span { get; }
        public bool IsMut { get; }
        public override string ToString() => Name;
    }

    public struct LabelSymbol
    {
        public LabelSymbol(string name) => Name = name;
        public string Name { get; }
        public override string ToString() => Name;
    }

    public struct TextLine
    {
        public TextLine(SourceText source, int start, int length, int lengthWithLineBreak)
        {
            Source = source;
            Start = start;
            Length = length;
            LengthWithLineBreak = lengthWithLineBreak;
        }

        public SourceText Source { get; }
        public int Start { get; }
        public int Length { get; }
        public int LengthWithLineBreak { get; }
        public int End => Start + Length;
        public TextSpan Span => new(Start, Length);
        public TextSpan SpanWithLineBreak => new(Start, LengthWithLineBreak);
        public override string ToString() => Source.ToString(Span);
    }

    public sealed class SourceText
    {
        private readonly string _text;
        private SourceText(string text)
        {
            _text = text;
            Lines = ParseLines(this, text);
        }

        public char this[int index] => _text[index];

        public static SourceText From(string text) => new(text);
        public ImmutableArray<TextLine> Lines { get; }
        public int Length => _text.Length;
        private static void AddLine(ImmutableArray<TextLine>.Builder result, SourceText sourceText, int lineStart, int position, int lineBreakWidth)
            => result.Add(new(sourceText, lineStart, position - lineStart, position - lineStart + lineBreakWidth));

        private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
        {
            ImmutableArray<TextLine>.Builder result = ImmutableArray.CreateBuilder<TextLine>();
            int lineStart = 0, position = 0;

            while (position <= text.Length)
            {
                int lineBreakWidth = GetLineBreakWidth(text, position);
                if (lineBreakWidth == 0) ++position;
                else
                {
                    AddLine(result, sourceText, lineStart, position, lineBreakWidth);
                    position += lineBreakWidth;
                    lineStart = position;
                }
            }

            if (position >= lineStart) AddLine(result, sourceText, lineStart, position, 0);
            return result.ToImmutable();
        }

        public int GetLineIndex(int position)
        {
            int lower = 0, upper = Lines.Length - 1;

            while (lower <= upper)
            {
                int index = lower + (upper - lower) / 2;
                int start = Lines[index].Start;

                if (position == start) return index;
                if (start > position)
                    upper = index - 1;
                else lower = index + 1;
            }

            return lower - 1;
        }
        private static int GetLineBreakWidth(string text, int position)
        {
            char c = text[Math.Clamp(position, 0, text.Length - 1)];
            int l = position + 1 >= text.Length ? '\0' : text[position + 1];
            if (c == '\r' && l == '\n') return 2;
            else if (c == '\r' || c == '\n') return 1;
            return 0;
        }

        public override string ToString() => _text;
        public string ToString(int start, int length) => _text.Substring(start, Math.Clamp(length + 1, 0, Length));
        public string ToString(TextSpan span) => ToString(span.Start, span.Length);
    }

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
