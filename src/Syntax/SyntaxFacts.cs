using Stonylang_CSharp.Lexer;
using System;

namespace Stonylang_CSharp.SyntaxFacts
{
    internal static class SyntaxFacts
    {
        public static int GetUnaryOpPrecedence(this TokenKind kind) => kind switch
        {
            TokenKind.Plus or TokenKind.Minus or TokenKind.Inv or TokenKind.Not => 10,
            _ => 0
        };
        public static int GetBinaryOpPrecedence(this TokenKind kind) => kind switch
        {
            TokenKind.Power => 9,
            TokenKind.Star or TokenKind.Slash => 8,
            TokenKind.Plus or TokenKind.Minus => 7,
            TokenKind.EqEq or TokenKind.NotEq => 6,
            TokenKind.And => 5,
            TokenKind.Xor => 4,
            TokenKind.Or => 3,
            TokenKind.LogicalAnd => 2,
            TokenKind.LogicalOr => 1,
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
