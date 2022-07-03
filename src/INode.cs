using Stonylang_CSharp.Lexer;
using System.Collections.Generic;

namespace Stonylang_CSharp.Parser
{
    public interface INode
    {
        public abstract TokenKind Kind { get; }
        public abstract IEnumerable<INode> GetChildren();
    }

    public abstract class ExprNode : INode
    {
        public abstract TokenKind Kind { get; }
        public abstract IEnumerable<INode> GetChildren();
    }

    public sealed class LiteralExpr : ExprNode
    {
        public LiteralExpr(Token literalToken) => LiteralToken = literalToken;
        public override TokenKind Kind => TokenKind.NumberExpr;
        public Token LiteralToken { get; }

        public override IEnumerable<INode> GetChildren()
        {
            yield return LiteralToken;
        }
    }

    public sealed class GroupingExpr : ExprNode
    {
        public GroupingExpr(Token lParen, ExprNode expr, Token rParen)
        {
            LParen = lParen;
            Expr = expr;
            RParen = rParen;
        }

        public override TokenKind Kind => TokenKind.GroupingExpr;

        public Token LParen { get; }
        public ExprNode Expr { get; }
        public Token RParen { get; }

        public override IEnumerable<INode> GetChildren()
        {
            yield return LParen;
            yield return Expr;
            yield return RParen;
        }
    }

    public sealed class BinaryExpr : ExprNode
    {
        public BinaryExpr(ExprNode left, INode op, ExprNode right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public override TokenKind Kind => TokenKind.BinaryExpr;
        public ExprNode Left { get; }
        public INode Op { get; }
        public ExprNode Right { get; }

        public override IEnumerable<INode> GetChildren()
        {
            yield return Left;
            yield return Op;
            yield return Right;
        }
    }

    public sealed class UnaryExpr : ExprNode
    {
        public UnaryExpr(ExprNode operand, INode op)
        {
            Operand = operand;
            Op = op;
        }

        public override TokenKind Kind => TokenKind.UnaryExpr;

        public ExprNode Operand { get; }
        public INode Op { get; }

        public override IEnumerable<INode> GetChildren()
        {
            yield return Operand;
            yield return Op;
        }
    }
}
