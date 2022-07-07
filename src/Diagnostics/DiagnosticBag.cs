using System.Collections;
using System.Collections.Generic;

namespace Stonylang_CSharp.Utility
{
    public sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new();
        public void Report(SourceText source, TextSpan span, string message, string errorType, LogLevel level) => _diagnostics.Add(new(source, span, errorType, message, level));
        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public DiagnosticBag AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
            return this;
        }
    }
}
