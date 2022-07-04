using Stonylang_CSharp.Lexer;
using System;

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

        internal static TokenKind GetKeywordKind(string text) => text switch
        {
            "true" => TokenKind.True,
            "false" => TokenKind.False,
            "var" => TokenKind.Var,
            "mut" => TokenKind.Mut,
            "fn" => TokenKind.Fn,
            _ => TokenKind.Identifier
        };
    }
}
