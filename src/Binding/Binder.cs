using Stonylang_CSharp.Diagnostics;
using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Parser;
using System;
using System.Collections.Generic;

namespace Stonylang_CSharp.Binding
{
    internal sealed class BoundUnaryOperator
    {
        private BoundUnaryOperator(TokenKind kind, BoundUnaryOpKind opKind, Type operandType)
            : this(kind, opKind, operandType, operandType) { }
        private BoundUnaryOperator(TokenKind kind, BoundUnaryOpKind opKind, Type operandType, Type resultType)
        {
            Kind = kind;
            OpKind = opKind;
            OperandType = operandType;
            ResultType = resultType;
        }

        public TokenKind Kind { get; }
        public BoundUnaryOpKind OpKind { get; }
        public Type OperandType { get; }
        public Type ResultType { get; }

        private static readonly BoundUnaryOperator[] _operators =
        {
            new(TokenKind.Not, BoundUnaryOpKind.LogicalNegation, typeof(bool)),
            new(TokenKind.Plus, BoundUnaryOpKind.Identity, typeof(int)),
            new(TokenKind.Minus, BoundUnaryOpKind.Negation, typeof(int)),
            new(TokenKind.Inv, BoundUnaryOpKind.Inv, typeof(int))
        };

        public static BoundUnaryOperator Bind(TokenKind kind, Type operandType)
        {
            foreach (var op in _operators)
                if (op.Kind == kind && op.OperandType == operandType) return op;
            return null;
        }
    }

    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(TokenKind kind, BoundBinaryOpKind opKind, Type type)
            : this(kind, opKind, type, type, type) { }
        private BoundBinaryOperator(TokenKind kind, BoundBinaryOpKind opKind, Type operandType, Type type)
            : this(kind, opKind, operandType, operandType, type) { }
        private BoundBinaryOperator(TokenKind kind, BoundBinaryOpKind opKind, Type leftType, Type rightType, Type resultType)
        {
            Kind = kind;
            OpKind = opKind;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        public TokenKind Kind { get; }
        public BoundBinaryOpKind OpKind { get; }
        public Type LeftType { get; }
        public Type RightType { get; }
        public Type ResultType { get; }

        private static readonly BoundBinaryOperator[] _operators =
        {
            // int
            new(TokenKind.Plus, BoundBinaryOpKind.Addition, typeof(int)),
            new(TokenKind.Minus, BoundBinaryOpKind.Subtraction, typeof(int)),
            new(TokenKind.Star, BoundBinaryOpKind.Multiplication, typeof(int)),
            new(TokenKind.Slash, BoundBinaryOpKind.Division, typeof(int)),
            new(TokenKind.Power, BoundBinaryOpKind.Power, typeof(int)),
            new(TokenKind.Or, BoundBinaryOpKind.Or, typeof(int)),
            new(TokenKind.And, BoundBinaryOpKind.And, typeof(int)),
            new(TokenKind.Xor, BoundBinaryOpKind.Xor, typeof(int)),

            new(TokenKind.EqEq, BoundBinaryOpKind.LogicalEq, typeof(int), typeof(bool)),
            new(TokenKind.NotEq, BoundBinaryOpKind.LogicalNotEq, typeof(int), typeof(bool)),

            // bool
            new(TokenKind.LogicalAnd, BoundBinaryOpKind.LogicalAnd, typeof(bool)),
            new(TokenKind.LogicalOr, BoundBinaryOpKind.LogicalOr, typeof(bool)),
            new(TokenKind.EqEq, BoundBinaryOpKind.LogicalEq, typeof(bool)),
            new(TokenKind.NotEq, BoundBinaryOpKind.LogicalNotEq, typeof(bool)),
            new(TokenKind.Or, BoundBinaryOpKind.Or, typeof(bool)),
            new(TokenKind.And, BoundBinaryOpKind.And, typeof(bool)),
            new(TokenKind.Xor, BoundBinaryOpKind.Xor, typeof(bool))
        };

        public static BoundBinaryOperator Bind(TokenKind kind, Type leftType, Type rightType)
        {
            foreach (var op in _operators)
                if (op.Kind == kind && op.LeftType == leftType && op.RightType == rightType) return op;
            return null;
        }
    }

    internal sealed class Binder
    {
        private readonly string _source;
        private readonly DiagnosticBag _diagnostics = new();
        public DiagnosticBag Diagnostics => _diagnostics;
        public Binder(string source) => _source = source;

        public BoundExpr BindExpr(ExprNode expr) => expr.Kind switch
        {
            TokenKind.LiteralExpr => BindLiteralExpr((LiteralExpr)expr),
            TokenKind.UnaryExpr => BindUnaryExpr((UnaryExpr)expr),
            TokenKind.BinaryExpr => BindBinaryExpr((BinaryExpr)expr),
            _ => throw new Exception($"Unexpected syntax \"{expr.Kind}\""),
        };

        private static BoundExpr BindLiteralExpr(LiteralExpr expr) => new BoundLiteralExpr(expr.Value ?? 0);
        private BoundExpr BindUnaryExpr(UnaryExpr expr)
        {
            BoundExpr boundOperand = BindExpr(expr.Operand);
            BoundUnaryOperator boundOperator = BoundUnaryOperator.Bind(expr.Op.Kind, boundOperand.Type);
            if (boundOperator != null)
                return new BoundUnaryExpr(boundOperator, boundOperand);

            _diagnostics.Report(_source, expr.Op.Span, expr.Op.Line, $"Unary operator '{expr.Op.Lexeme}' is not defined for type \"{boundOperand.Type}\".", "TypeException", LogLevel.Error);
            return boundOperand;
        }
        private BoundExpr BindBinaryExpr(BinaryExpr expr)
        {
            BoundExpr boundLeft = BindExpr(expr.Left);
            BoundExpr boundRight = BindExpr(expr.Right);
            BoundBinaryOperator boundOperator = BoundBinaryOperator.Bind(expr.Op.Kind, boundLeft.Type, boundRight.Type);
            if (boundOperator != null)
                return new BoundBinaryExpr(boundLeft, boundOperator, boundRight);

            _diagnostics.Report(_source, expr.Op.Span, expr.Op.Line, $"Binary operator '{expr.Op.Lexeme}' is not defined for types \"{boundLeft.Type}\" and \"{boundRight.Type}\".", "TypeException", LogLevel.Error);
            return boundLeft;
        }
    }
}
