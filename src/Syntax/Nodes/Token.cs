using Stonylang.Parser;
using Stonylang.Utility;

namespace Stonylang.Lexer
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
        Int, Float, String, Identifier,

        // Keywords
        FnKeyword, ClassKeyword, IfKeyword, ElseKeyword, SwitchKeyword, CaseKeyword, DefaultKeyword,

        VarKeyword, MutKeyword, BreakKeyword, ContinueKeyword, TrueKeyword, FalseKeyword, ReturnKeyword,
        WhileKeyword, DoKeyword, ForKeyword, AsyncKeyword, AwaitKeyword, GoToKeyword, ToKeyword,

        // Others
        Semicolon, Comma, LParen, RParen, LBrace, RBrace,

        LBracket, RBracket, Arrow, QuestionMark, Colon,

        // Exprs
        LiteralExpr, GroupingExpr, UnaryExpr, BinaryExpr,

        NameExpr, AssignmentExpr, CallExpr,

        // Stmts
        ExpressionStmt, BlockStmt, VariableStmt,

        IfStmt, ElseClauseStmt, WhileStmt, ForStmt,

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
        public bool IsMissing => string.IsNullOrEmpty(Lexeme);

        public new string ToString() => $"Kind: {Kind}\nLexeme: {Lexeme}\nPosition: [{Span.Start}-{Span.Start + Span.Length}]{(Literal != null ? $"\nLiteral: {Literal}" : "")}";
    }
}
