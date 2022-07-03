using Stonylang_CSharp.Lexer;

namespace Stonylang_CSharp.SyntaxFacts
{
    internal static class SyntaxFacts
    {
        public static int GetUnaryOpPrecedence(this TokenKind kind) => kind switch
        {
            TokenKind.Plus or TokenKind.Minus or TokenKind.Inv => 3,
            _ => 0
        };
        public static int GetBinaryOpPrecedence(this TokenKind kind) => kind switch
        {
            TokenKind.Star or TokenKind.Slash => 2,
            TokenKind.Plus or TokenKind.Minus => 1,
            _ => 0
        };
    }
}
