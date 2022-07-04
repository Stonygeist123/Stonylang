using System;

namespace Stonylang_CSharp.Binding
{
    internal enum BoundNodeKind
    {
        LiteralExpr,
        UnaryExpr,
        BinaryExpr
    }

    internal enum BoundUnaryOpKind
    {
        Identity,
        Negation
    }

    internal enum BoundBinaryOpKind
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }

    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
    }

    internal abstract class BoundExpr : BoundNode
    {
        public abstract Type Type { get; }
    }

    internal sealed class BoundLiteralExpr : BoundExpr
    {
        public BoundLiteralExpr(object value) => Value = value;
        public override Type Type => Value.GetType();
        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpr;
        public object Value { get; }

    }

    internal sealed class BoundUnaryExpr : BoundExpr
    {
        public BoundUnaryExpr(BoundUnaryOpKind opKind, BoundExpr operand)
        {
            OpKind = opKind;
            Operand = operand;
        }

        public override Type Type => Operand.Type;
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpr;
        public BoundUnaryOpKind OpKind { get; }
        public BoundExpr Operand { get; }
    }

    internal sealed class BoundBinaryExpr : BoundExpr
    {
        public BoundBinaryExpr(BoundExpr left, BoundBinaryOpKind opKind, BoundExpr right)
        {
            Left = left;
            OpKind = opKind;
            Right = right;
        }

        public override Type Type => Left.Type;
        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpr;
        public BoundExpr Left { get; }
        public BoundBinaryOpKind OpKind { get; }
        public BoundExpr Right { get; }
    }
}
