using Stonylang.Lexer;
using Stonylang.Utility;

namespace Stonylang.Parser
{
    public abstract class ExprNode : Node
    { }

    public sealed class LiteralExpr : ExprNode
    {
        public LiteralExpr(Token literalToken) : this(literalToken, literalToken.Literal)
        {
        }

        public LiteralExpr(Token literalToken, object value)
        {
            LiteralToken = literalToken;
            Value = value;
        }

        public override SyntaxKind Kind => SyntaxKind.LiteralExpr;
        public Token LiteralToken { get; }
        public object Value { get; }
    }

    public sealed class UnaryExpr : ExprNode
    {
        public UnaryExpr(Token op, ExprNode operand)
        {
            Operand = operand;
            Op = op;
        }

        public override SyntaxKind Kind => SyntaxKind.UnaryExpr;
        public Token Op { get; }
        public ExprNode Operand { get; }
    }

    public sealed class BinaryExpr : ExprNode
    {
        public BinaryExpr(ExprNode left, Token op, ExprNode right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpr;
        public ExprNode Left { get; }
        public Token Op { get; }
        public ExprNode Right { get; }
    }

    public sealed class NameExpr : ExprNode
    {
        public NameExpr(Token name) => Name = name;

        public override SyntaxKind Kind => SyntaxKind.NameExpr;
        public Token Name { get; }
    }

    public sealed class AssignmentExpr : ExprNode
    {
        public AssignmentExpr(Token name, Token equalsToken, ExprNode value)
        {
            Name = name;
            EqualsToken = equalsToken;
            Value = value;
        }

        public override SyntaxKind Kind => SyntaxKind.AssignmentExpr;
        public Token Name { get; }
        public Token EqualsToken { get; }
        public ExprNode Value { get; }
    }

    public sealed class CallExpr : ExprNode
    {
        public CallExpr(Token identifier, Token lParen, SeparatedSyntaxList<ExprNode> arguments, Token rParen)
        {
            Identifier = identifier;
            LParen = lParen;
            Arguments = arguments;
            RParen = rParen;
        }

        public override SyntaxKind Kind => SyntaxKind.CallExpr;
        public Token Identifier { get; }
        public Token LParen { get; }
        public SeparatedSyntaxList<ExprNode> Arguments { get; }
        public Token RParen { get; }
    }
}
