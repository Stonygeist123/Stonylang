using Stonylang_CSharp.Lexer;
using System;

namespace Stonylang_CSharp.SyntaxFacts
{
    internal static class SyntaxFacts
    {
        public static int GetUnaryOpPrecedence(this SyntaxKind kind) => kind switch
        {
            SyntaxKind.Plus or SyntaxKind.Minus or SyntaxKind.Inv or SyntaxKind.Not => 10,
            _ => 0
        };
        public static int GetBinaryOpPrecedence(this SyntaxKind kind) => kind switch
        {
            SyntaxKind.Power => 9,
            SyntaxKind.Star or SyntaxKind.Slash => 8,
            SyntaxKind.Plus or SyntaxKind.Minus => 7,
            SyntaxKind.EqEq or SyntaxKind.NotEq => 6,
            SyntaxKind.And => 5,
            SyntaxKind.Xor => 4,
            SyntaxKind.Or => 3,
            SyntaxKind.LogicalAnd => 2,
            SyntaxKind.LogicalOr => 1,
            _ => 0
        };

        internal static SyntaxKind GetKeywordKind(string text) => text switch
        {
            "true" => SyntaxKind.True,
            "false" => SyntaxKind.False,
            "var" => SyntaxKind.Var,
            "mut" => SyntaxKind.Mut,
            "fn" => SyntaxKind.Fn,
            _ => SyntaxKind.Identifier
        };
    }
}
