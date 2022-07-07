using Stonylang_CSharp.Parser;
using Stonylang_CSharp.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Stonylang_CSharp.Lexer
{
    public enum SyntaxKind
    {
        // Arithmetic
        Plus, Minus, Star, Slash, Power, Mod, Increment, Decrement,

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
        Var, Mut, Break, Continue, True, False, Return,
        While, Do, For, ForEach, Async, Await, GoTo,

        // Others
        Semicolon, Comma, LParen, RParen, LBrace, RBrace,
        LBracket, RBracket, Arrow, QuestionMark, Colon,

        // Exprs
        LiteralExpr, GroupingExpr, UnaryExpr, BinaryExpr,
        NameExpr, AssignmentExpr,

        // Stmts
        ExpressionStmt, BlockStmt, VariableStmt,

        // Nodes
        CompilationUnit,

        Whitespace, Bad, EOF
    }
    public class Token : Node
    {
        public Token(SyntaxKind _kind, string _lexeme, object _literal, TextSpan _span, int _line)
        {
            Kind = _kind;
            Lexeme = _lexeme;
            Literal = _literal;
            Span = _span;
            Line = _line;
        }

        public override SyntaxKind Kind { get; }
        public string Lexeme { get; }
        public object Literal { get; }
        public int Line { get; }
        public override TextSpan Span { get; }
        public new string ToString() => $"Kind: {Kind}\nLexeme: {Lexeme}\nPosition: [{Span.Start}-{Span.Start + Span.Length}]{(Literal != null ? $"\nLiteral: {Literal}" : "")}";
    }
}
