using System;
using System.Collections;
using System.Collections.Generic;

namespace Stonylang_CSharp.Utility
{
    public sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new();
        public void Report(SourceText source, TextSpan span, string msg, string errorType, LogLevel level)
        {
            int lineIndex = source.GetLineIndex(span.Start);
            TextLine line = source.Lines[lineIndex];
            int c = span.Start - line.Start + 1;
            string levelS = level switch
            {
                LogLevel.Error => "Error",
                LogLevel.Warn => "Warn ",
                LogLevel.Info => "Info ",
                _ => "Unknown Exception"
            };
            string msg1 = $"[{levelS} {lineIndex + 1}:{c}{(span.End == (span.Start + 1) ? "" : $"-{span.End}")}] ", message = msg1 + line.ToString() + "\n";
            string spacing = new string(' ', span.Start + msg1.Length);
            message += spacing + new string('^', span.Length) + '\n';
            message += spacing + $"{errorType}: {msg}";
            _diagnostics.Add(new(span, message));
        }

        public DiagnosticBag AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
            return this;
        }
        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
