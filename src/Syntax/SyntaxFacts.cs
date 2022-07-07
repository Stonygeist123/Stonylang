using Stonylang_CSharp.Lexer;
using System;
using System.Collections.Generic;

namespace Stonylang_CSharp.SyntaxFacts
{
    public static class SyntaxFacts
    {
        public static int GetUnaryOpPrecedence(this SyntaxKind kind) => kind switch
        {
            SyntaxKind.Increment or SyntaxKind.Decrement => 13,
            SyntaxKind.Plus or SyntaxKind.Minus or SyntaxKind.Inv or SyntaxKind.Not => 12,
            _ => 0
        };
        public static int GetBinaryOpPrecedence(this SyntaxKind kind) => kind switch
        {
            SyntaxKind.Power => 11,
            SyntaxKind.Star or SyntaxKind.Slash or SyntaxKind.Mod => 10,
            SyntaxKind.Plus or SyntaxKind.Minus => 9,
            SyntaxKind.Rsh or SyntaxKind.Lsh => 8,
            SyntaxKind.Greater or SyntaxKind.GreaterEq or SyntaxKind.Less or SyntaxKind.LessEq => 7,
            SyntaxKind.EqEq or SyntaxKind.NotEq => 6,
            SyntaxKind.And => 5,
            SyntaxKind.Xor => 4,
            SyntaxKind.Or => 3,
            SyntaxKind.LogicalAnd => 2,
            SyntaxKind.LogicalOr => 1,
            _ => 0
        };

        public static SyntaxKind GetKeywordKind(string text) => text switch
        {
            "true" => SyntaxKind.True,
            "false" => SyntaxKind.False,
            "var" => SyntaxKind.Var,
            "mut" => SyntaxKind.Mut,
            "if" => SyntaxKind.If,
            "else" => SyntaxKind.Else,
            "switch" => SyntaxKind.Switch,
            "case" => SyntaxKind.Case,
            "default" => SyntaxKind.Default,
            "break" => SyntaxKind.Break,
            "continue" => SyntaxKind.Continue,
            "fn" => SyntaxKind.Fn,
            "class" => SyntaxKind.Class,
            "return" => SyntaxKind.Return,
            "while" => SyntaxKind.While,
            "do" => SyntaxKind.Do,
            "for" => SyntaxKind.For,
            "foreach" => SyntaxKind.ForEach,
            "async" => SyntaxKind.Async,
            "await" => SyntaxKind.Await,
            "goto" => SyntaxKind.GoTo,
            _ => SyntaxKind.Identifier
        };

        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
        {
            SyntaxKind[] kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (SyntaxKind kind in kinds)
                if (GetUnaryOpPrecedence(kind) > 0)
                    yield return kind;
        }

        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            SyntaxKind[] kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (SyntaxKind kind in kinds)
                if (GetBinaryOpPrecedence(kind) > 0)
                    yield return kind;
        }

        public static string GetText(this SyntaxKind kind) => kind switch
        {
            SyntaxKind.Plus => "+",
            SyntaxKind.Minus => "-",
            SyntaxKind.Star => "*",
            SyntaxKind.Slash => "/",
            SyntaxKind.Power => "**",
            SyntaxKind.Mod => "%",
            SyntaxKind.Increment => "++",
            SyntaxKind.Decrement => "--",
            SyntaxKind.EqEq => "==",
            SyntaxKind.NotEq => "!=",
            SyntaxKind.Greater => ">",
            SyntaxKind.GreaterEq => ">=",
            SyntaxKind.Less => "<",
            SyntaxKind.LessEq => "<=",
            SyntaxKind.Rsh => ">>",
            SyntaxKind.Lsh => "<<",
            SyntaxKind.And => "&",
            SyntaxKind.Xor => "^",
            SyntaxKind.Or => "|",
            SyntaxKind.LogicalOr => "||",
            SyntaxKind.LogicalAnd => "&&",
            SyntaxKind.Equals => "=",
            SyntaxKind.PlusEq => "+=",
            SyntaxKind.MinusEq => "-=",
            SyntaxKind.StarEq => "*=",
            SyntaxKind.SlashEq => "/=",
            SyntaxKind.PowerEq => "**=",
            SyntaxKind.ModEq => "%=",
            SyntaxKind.XorEq => "^=",
            SyntaxKind.OrEq => "|=",
            SyntaxKind.AndEq => "&=",
            SyntaxKind.LshEq => "<<=",
            SyntaxKind.RshEq => ">>=",
            SyntaxKind.Inv => "~",
            SyntaxKind.Not => "!",
            SyntaxKind.Fn => "fn",
            SyntaxKind.Class => "class",
            SyntaxKind.If => "if",
            SyntaxKind.Else => "else",
            SyntaxKind.Switch => "switch",
            SyntaxKind.Case => "case",
            SyntaxKind.Default => "default",
            SyntaxKind.Var => "var",
            SyntaxKind.Mut => "mut",
            SyntaxKind.Break => "break",
            SyntaxKind.Continue => "continue",
            SyntaxKind.True => "true",
            SyntaxKind.False => "false",
            SyntaxKind.Return => "return",
            SyntaxKind.While => "while",
            SyntaxKind.Do => "do",
            SyntaxKind.For => "for",
            SyntaxKind.ForEach => "foreach",
            SyntaxKind.Async => "async",
            SyntaxKind.Await => "await",
            SyntaxKind.GoTo => "goto",
            SyntaxKind.Semicolon => ";",
            SyntaxKind.Comma => ",",
            SyntaxKind.LParen => "(",
            SyntaxKind.RParen => ")",
            SyntaxKind.LBrace => "{",
            SyntaxKind.RBrace => "}",
            SyntaxKind.LBracket => "[",
            SyntaxKind.RBracket => "]",
            SyntaxKind.Arrow => "->",
            SyntaxKind.QuestionMark => "?",
            SyntaxKind.Colon => ":",
            SyntaxKind.EOF => "End of File",
            _ => null
        };
    }
}
