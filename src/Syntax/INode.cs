using Stonylang_CSharp.Lexer;
using System.Collections.Generic;

namespace Stonylang_CSharp.Parser
{
    public interface INode
    {
        public abstract SyntaxKind Kind { get; }
        public abstract IEnumerable<INode> GetChildren();
    }

    public abstract class ExprNode : INode
    {
        public abstract SyntaxKind Kind { get; }
        public abstract IEnumerable<INode> GetChildren();
    }

    public sealed class LiteralExpr : ExprNode
    {
        public LiteralExpr(Token literalToken) : this(literalToken, literalToken.Literal) { }
        public LiteralExpr(Token literalToken, object value)
        {
            LiteralToken = literalToken;
            Value = value;
        }

        public override SyntaxKind Kind => SyntaxKind.LiteralExpr;
        public Token LiteralToken { get; }
        public object Value { get; }

        public override IEnumerable<INode> GetChildren()
        {
            yield return LiteralToken;
        }
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

        public override IEnumerable<INode> GetChildren()
        {
            yield return Op;
            yield return Operand;
        }
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

        public override IEnumerable<INode> GetChildren()
        {
            yield return Left;
            yield return Op;
            yield return Right;
        }
    }

    public sealed class NameExpr : ExprNode
    {
        public NameExpr(Token name) => Name = name;
        public override SyntaxKind Kind => SyntaxKind.NameExpr;
        public Token Name { get; }

        public override IEnumerable<INode> GetChildren()
        {
            yield return Name;
        }
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

        public override IEnumerable<INode> GetChildren()
        {
            yield return Name;
            yield return EqualsToken;
            yield return Value;
        }
    }
}
