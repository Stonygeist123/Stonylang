using Stonylang.Binding;
using Stonylang.Symbols;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stonylang.Utility
{
    public enum LogLevel
    {
        Info, Warn, Error
    }

    public sealed class BoundLabel
    {
        public BoundLabel(string name) => Name = name;
        public string Name { get; }
        public override string ToString() => Name;
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
            char c = text[Math.Clamp(position, 0, Math.Clamp(text.Length - 1, 0, text.Length - 1))];
            int l = position + 1 >= text.Length ? '\0' : text[position + 1];
            if (c == '\r' && l == '\n') return 2;
            else if (c == '\r' || c == '\n') return 1;
            return 0;
        }

        public override string ToString() => _text;
        public string ToString(int start, int length) => _text.Substring(start, Math.Clamp(length + 1, 0, Length));
        public string ToString(TextSpan span) => ToString(span.Start, span.Length);
    }
}
