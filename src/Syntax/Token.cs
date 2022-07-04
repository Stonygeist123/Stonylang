using Stonylang_CSharp.Parser;
using System;
using System.Linq;
using System.Collections.Generic;
using Stonylang_CSharp.Diagnostics;

namespace Stonylang_CSharp.Lexer
{

    public enum TokenKind
    {
        // Arithmetic
        Plus, Minus, Star, Slash, Mod, Power, Increment, Decrement,

        // Relational
        EqEq, NotEq, Greater, GreaterEq, Less, LessEq,

        // Bitshift
        Rsh, Lsh,

        // Bitwise
        And, Xor, Or,

        // Logical
        LogicalOr, LogicalAnd,

        // Assignment
        Equals, PlusEq, MinusEq, StarEq, SlashEq, PowerEq,
        ModEq, XorEq, OrEq, AndEq, LshEq, RshEq,

        // Strict Unary
        Inv,
        Not,

        // Literals
        Number, String, Identifier,

        // Keywords
        Fn, Class, If, Else, Switch, Case, Default,
        Var, Mut, Break, Continue, True, False, Null,
        Return, While, Do, For, ForEach, Async, Await,
        GoTo,

        // Others
        Semicolon, Comma, LParen, RParen, LBrace, RBrace,
        LBracket, RBracket, Arrow, QuestionMark, Colon,

        // Exprs
        LiteralExpr, GroupingExpr, UnaryExpr, BinaryExpr,

        Whitespace, Bad, EOF
    }
    public struct Token : INode
    {
        public Token(TokenKind _kind, string _lexeme, object _literal, TextSpan _span, int _line)
        {
            Kind = _kind;
            Lexeme = _lexeme;
            Literal = _literal;
            Span = _span;
            Line = _line;
        }

        public TokenKind Kind { get; }
        public string Lexeme { get; }
        public object Literal { get; }
        public TextSpan Span { get; }
        public int Line { get; }

        public IEnumerable<INode> GetChildren() => Enumerable.Empty<INode>();
        public override string ToString()
            => $"Kind: {Kind}\nLexeme: {Lexeme}\nPosition: [{Span.Start}-{Span.Start + Span.Length}]{(Literal != null ? $"\nLiteral: {Literal}" : "")}";
    }
}
