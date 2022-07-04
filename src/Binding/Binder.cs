using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Parser;
using System;
using System.Collections.Generic;

namespace Stonylang_CSharp.Binding
{
    internal sealed class Binder
    {
        private readonly List<string> _diagnostics = new();
        public IEnumerable<string> Diagnostics => _diagnostics;

        public BoundExpr BindExpr(ExprNode expr) => expr.Kind switch
        {
            TokenKind.LiteralExpr => BindLiteralExpr((LiteralExpr)expr),
            TokenKind.UnaryExpr => BindUnaryExpr((UnaryExpr)expr),
            TokenKind.BinaryExpr => BindBinaryExpr((BinaryExpr)expr),
            _ => throw new Exception($"Unexpected syntax <{expr.Kind}>"),
        };

        private BoundExpr BindLiteralExpr(LiteralExpr expr) => new BoundLiteralExpr(expr.Value ?? 0);
        private BoundExpr BindUnaryExpr(UnaryExpr expr)
        {
            BoundExpr boundOperand = BindExpr(expr.Operand);
            BoundUnaryOpKind? opKind = BindUnaryOpKind(expr.Op.Kind, boundOperand.Type);
            if (opKind != null)
                return new BoundUnaryExpr((BoundUnaryOpKind)opKind, boundOperand);
            _diagnostics.Add($"Unary operator '{expr.Op.Lexeme}' is not defined for type <{boundOperand.Type}>.");
            return boundOperand;
        }
        private BoundExpr BindBinaryExpr(BinaryExpr expr)
        {
            BoundExpr boundLeft = BindExpr(expr.Left);
            BoundExpr boundRight = BindExpr(expr.Right);
            BoundBinaryOpKind? opKind = BindBinaryOpKind(expr.Op.Kind, boundLeft.Type, boundRight.Type);
            if (opKind != null)
                return new BoundBinaryExpr(boundLeft, (BoundBinaryOpKind)opKind, boundRight);
            _diagnostics.Add($"Binary operator '{expr.Op.Lexeme}' is not defined for types <{boundLeft.Type}> and <{boundRight.Type}>.");
            return boundLeft;
        }

        private BoundUnaryOpKind? BindUnaryOpKind(TokenKind kind, Type operandType)
        {
            if (operandType != typeof(int)) return null;
            return kind switch
            {
                TokenKind.Plus => BoundUnaryOpKind.Identity,
                TokenKind.Minus => BoundUnaryOpKind.Negation,
                _ => throw new Exception($"Unexpected unary operator <{kind}>."),
            };
        }

        private BoundBinaryOpKind? BindBinaryOpKind(TokenKind kind, Type leftType, Type rightType)
        {
            if (leftType != typeof(int) || rightType != typeof(int)) return null;
            return kind switch
            {
                TokenKind.Plus => BoundBinaryOpKind.Addition,
                TokenKind.Minus => BoundBinaryOpKind.Subtraction,
                TokenKind.Star => BoundBinaryOpKind.Multiplication,
                TokenKind.Slash => BoundBinaryOpKind.Division,
                _ => throw new Exception($"Unexpected binary operator <{kind}>."),
            };
        }
    }
}
