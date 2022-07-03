using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Parser;
using System;

namespace Stonylang_CSharp.Evaluator
{
    public sealed class Evaluator
    {
        private readonly ExprNode _root;

        public Evaluator(ExprNode root) => _root = root;

        public int Evaluate() => EvaluateExpression(_root);
        private int EvaluateExpression(ExprNode node)
        {
            if (node is LiteralExpr n) return (int)n.LiteralToken.Literal;
            if (node is BinaryExpr b)
            {
                int left = EvaluateExpression(b.Left);
                int right = EvaluateExpression(b.Right);

                return b.Op.Kind switch
                {
                    TokenKind.Plus => left + right,
                    TokenKind.Minus => left - right,
                    TokenKind.Star => left * right,
                    TokenKind.Slash => left / right,
                    _ => throw new Exception($"Unexpected binary operator {b.Op.Kind}.")
                };
            }

            if (node is GroupingExpr g) return EvaluateExpression(g.Expr);
            throw new Exception($"Unexpected expression operator {node.Kind}.");
        }
    }
}
