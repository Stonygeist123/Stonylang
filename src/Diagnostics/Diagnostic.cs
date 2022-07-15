using System;

namespace Stonylang.Utility
{
    public sealed class Diagnostic
    {
        public Diagnostic(SourceText source, TextSpan span, string errorType, string message, LogLevel level)
        {
            Source = source;
            Span = span;
            Type = errorType;
            Message = message;
            Level = level;
        }

        public SourceText Source { get; }
        public TextSpan Span { get; }
        public string Type { get; }
        public string Message { get; }
        public LogLevel Level { get; }

        public void Print()
        {
            int lineIndex = Source.GetLineIndex(Span.Start);
            TextLine line = Source.Lines[lineIndex];
            int c = Span.Start - line.Start + 1;
            string levelS = Level switch
            {
                LogLevel.Error => "Error",
                LogLevel.Warn => "Warn ",
                LogLevel.Info => "Info ",
                _ => "Unknown Exception"
            };
            string msg = $"[{levelS} {lineIndex + 1}:{c}{(Span.End <= c ? "" : $"-{Span.End}")}] ",
                message = msg + line.ToString() + "\n",
                spacing = new(' ', Span.Start + msg.Length);
            message += spacing + new string('^', Span.Length) + '\n';
            message += spacing + $"{Type}: {Message}";

            Console.ForegroundColor = GetLogLevelColor();
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public override string ToString() => Message;

        public ConsoleColor GetLogLevelColor() => Level switch
        {
            LogLevel.Info => ConsoleColor.Cyan,
            LogLevel.Warn => ConsoleColor.DarkYellow,
            LogLevel.Error or _ => ConsoleColor.DarkRed,
        };
    }
}
