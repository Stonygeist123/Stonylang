using Stonylang_CSharp.Lexer;
using System.Collections.Generic;

namespace Stonylang_CSharp.Parser
{
    interface INode
    {
        public abstract TokenKind Kind { get; }
        public abstract IEnumerable<INode> GetChildren();
    }

    abstract class ExprNode : INode
    {
        public abstract TokenKind Kind { get; }
        public abstract IEnumerable<INode> GetChildren();
    }

    sealed class NumberExpr : ExprNode
    {
        public NumberExpr(Token numberToken) => NumberToken = numberToken;
        public override TokenKind Kind => TokenKind.NumberExpr;
        public Token NumberToken { get; }

        public override IEnumerable<INode> GetChildren()
        {
            yield return NumberToken;
        }
    }

    sealed class GroupingExpr : ExprNode
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

    sealed class BinaryExpr : ExprNode
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
}
