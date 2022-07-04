using Stonylang_CSharp.Binding;
using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Parser;
using System;

namespace Stonylang_CSharp.Evaluator
{
    internal sealed class Evaluator
    {
        private readonly BoundExpr _root;
        public Evaluator(BoundExpr root) => _root = root;

        public object Evaluate() => EvaluateExpression(_root);
        private object EvaluateExpression(BoundExpr node)
        {
            if (node is BoundLiteralExpr l) return l.Value;
            // if (node is BoundGroupingExpr g) return EvaluateExpression(g.Expr);
            if (node is BoundUnaryExpr u)
            {
                object operand = EvaluateExpression(u.Operand);
                if (operand is int @o)
                {
                    return u.OpKind switch
                    {
                        BoundUnaryOpKind.Identity => o,
                        BoundUnaryOpKind.Negation => -o,
                        _ => throw new Exception($"Unexpected unary operator <{u.OpKind}>.")
                    };
                }
            }
            if (node is BoundBinaryExpr b)
            {
                object left = EvaluateExpression(b.Left);
                object right = EvaluateExpression(b.Right);

                if (left is int @lO && right is int @rO)
                {
                    return b.OpKind switch
                    {
                        BoundBinaryOpKind.Addition => lO + rO,
                        BoundBinaryOpKind.Subtraction => lO - rO,
                        BoundBinaryOpKind.Multiplication => lO * rO,
                        BoundBinaryOpKind.Division => lO / rO,
                        _ => throw new Exception($"Unexpected binary operator <{b.OpKind}>.")
                    };
                }
            }

            throw new Exception($"Unexpected expression operator {node.Kind}.");
        }
    }
}
