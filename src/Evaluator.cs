﻿using Stonylang_CSharp.Lexer;
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
            if (node is GroupingExpr g) return EvaluateExpression(g.Expr);
            if (node is UnaryExpr u)
            {
                int operand = EvaluateExpression(u.Operand);
                return u.Op.Kind switch
                {
                    TokenKind.Plus => operand,
                    TokenKind.Minus => -operand,
                    TokenKind.Inv => ~operand,
                    _ => throw new Exception($"Unexpected unary operator <{u.Op.Kind}>.")

                };
            }
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
                    _ => throw new Exception($"Unexpected binary operator <{b.Op.Kind}>.")
                };
            }

            throw new Exception($"Unexpected expression operator {node.Kind}.");
        }
    }
}
