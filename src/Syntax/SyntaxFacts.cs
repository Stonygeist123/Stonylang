using Stonylang.Lexer;
using System;
using System.Collections.Generic;

namespace Stonylang.SyntaxFacts
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
            "true" => SyntaxKind.TrueKeyword,
            "false" => SyntaxKind.FalseKeyword,
            "var" => SyntaxKind.VarKeyword,
            "mut" => SyntaxKind.MutKeyword,
            "if" => SyntaxKind.IfKeyword,
            "else" => SyntaxKind.ElseKeyword,
            "while" => SyntaxKind.WhileKeyword,
            "do" => SyntaxKind.DoKeyword,
            "for" => SyntaxKind.ForKeyword,
            "switch" => SyntaxKind.SwitchKeyword,
            "case" => SyntaxKind.CaseKeyword,
            "default" => SyntaxKind.DefaultKeyword,
            "break" => SyntaxKind.BreakKeyword,
            "continue" => SyntaxKind.ContinueKeyword,
            "fn" => SyntaxKind.FnKeyword,
            "class" => SyntaxKind.ClassKeyword,
            "return" => SyntaxKind.ReturnKeyword,
            "async" => SyntaxKind.AsyncKeyword,
            "await" => SyntaxKind.AwaitKeyword,
            "goto" => SyntaxKind.GoToKeyword,
            "to" => SyntaxKind.ToKeyword,
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
            SyntaxKind.FnKeyword => "fn",
            SyntaxKind.ClassKeyword => "class",
            SyntaxKind.IfKeyword => "if",
            SyntaxKind.ElseKeyword => "else",
            SyntaxKind.SwitchKeyword => "switch",
            SyntaxKind.CaseKeyword => "case",
            SyntaxKind.DefaultKeyword => "default",
            SyntaxKind.VarKeyword => "var",
            SyntaxKind.MutKeyword => "mut",
            SyntaxKind.BreakKeyword => "break",
            SyntaxKind.ContinueKeyword => "continue",
            SyntaxKind.TrueKeyword => "true",
            SyntaxKind.FalseKeyword => "false",
            SyntaxKind.ReturnKeyword => "return",
            SyntaxKind.WhileKeyword => "while",
            SyntaxKind.DoKeyword => "do",
            SyntaxKind.ForKeyword => "for",
            SyntaxKind.AsyncKeyword => "async",
            SyntaxKind.AwaitKeyword => "await",
            SyntaxKind.GoToKeyword => "goto",
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
            _ => null
        };
    }
}
