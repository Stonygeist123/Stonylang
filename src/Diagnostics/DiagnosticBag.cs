using System;
using System.Collections;
using System.Collections.Generic;

namespace Stonylang_CSharp.Diagnostics
{
    public sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new();
        public void Report(string source, TextSpan span, int line, string msg, string errorType, LogLevel level)
        {
            string levelS = level switch
            {
                LogLevel.Error => "Error",
                LogLevel.Warn => "Warn ",
                LogLevel.Info => "Info ",
                _ => "Unknown Exception"
            };
            string msg1 = $"[{levelS} {line}:{span.Start + 1}-{span.End + 1}] ", message = msg1 + source.Split('\n')[line - 1] + "\n";
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
