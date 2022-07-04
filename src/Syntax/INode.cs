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
        public LiteralExpr(Token literalToken) : this(literalToken, literalToken.Literal) { }
        public LiteralExpr(Token literalToken, object value)
        {
            LiteralToken = literalToken;
            Value = value;
        }

        public override TokenKind Kind => TokenKind.LiteralExpr;
        public Token LiteralToken { get; }
        public object Value { get; }

        public override IEnumerable<INode> GetChildren()
        {
            yield return LiteralToken;
        }
    }

    /* public sealed class GroupingExpr : ExprNode
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
    } */

    public sealed class UnaryExpr : ExprNode
    {
        public UnaryExpr(Token op, ExprNode operand)
        {
            Operand = operand;
            Op = op;
        }

        public override TokenKind Kind => TokenKind.UnaryExpr;
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

        public override TokenKind Kind => TokenKind.BinaryExpr;
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
}
