namespace Stonylang_CSharp.Diagnostics
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
}
