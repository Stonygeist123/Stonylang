using Stonylang_CSharp.Parser;
using System;
using System.Linq;
using System.Collections.Generic;

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
        public Token(TokenKind _kind, string _lexeme, object _literal, Tuple<int, int> _position, int _line)
        {
            Kind = _kind;
            Lexeme = _lexeme;
            Literal = _literal;
            Position = _position;
            Line = _line;
        }

        public TokenKind Kind { get; }
        public string Lexeme { get; }
        public object Literal { get; }
        public Tuple<int, int> Position { get; }
        public int Line { get; }

        public IEnumerable<INode> GetChildren() => Enumerable.Empty<INode>();

        public override string ToString() => $"Kind: {Kind}\nLexeme: {Lexeme}\nPosition: [{Position.Item1}-{Position.Item2}]{(Literal != null ? $"\nLiteral: {Literal}" : "")}";
    }
}
