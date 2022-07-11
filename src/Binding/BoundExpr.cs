using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Utility;
using System;

namespace Stonylang_CSharp.Binding
{
    internal enum BoundNodeKind
    {
        // Exprs
        LiteralExpr,
        UnaryExpr,
        BinaryExpr,
        VariableExpr,
        AssignmentExpr,

        // Stmts
        BlockStatement,
        VariableStatement,
        IfStatement,
        WhileStatement,
        ForStatement,
        ExpressionStatement,
        GoToStatement,
        ConditionalGoToStatement,
        LabelStatement
    }

    internal enum BoundUnaryOpKind
    {
        Identity,
        Negation,
        Inv,
        LogicalNegation,
        Increment,
        Decrement
    }

    internal enum BoundBinaryOpKind
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Power,
        Modulo,
        LogicalAnd,
        LogicalOr,
        LogicalNotEq,
        LogicalEq,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        Greater,
        GreaterEq,
        Less,
        LessEq,
        Rsh,
        Lsh
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
        public BoundUnaryExpr(BoundUnaryOperator op, BoundExpr operand)
        {
            Op = op;
            Operand = operand;
        }

        public override Type Type => Op.ResultType;
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpr;
        public BoundUnaryOperator Op { get; }
        public BoundExpr Operand { get; }
    }

    internal sealed class BoundBinaryExpr : BoundExpr
    {
        public BoundBinaryExpr(BoundExpr left, BoundBinaryOperator op, BoundExpr right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public override Type Type => Op.ResultType;
        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpr;
        public BoundExpr Left { get; }
        public BoundBinaryOperator Op { get; }
        public BoundExpr Right { get; }
    }

    internal sealed class BoundVariableExpr : BoundExpr
    {
        public BoundVariableExpr(VariableSymbol variable) => Variable = variable;

        public VariableSymbol Variable { get; }
        public override Type Type => Variable.Type;
        public override BoundNodeKind Kind => BoundNodeKind.VariableExpr;
    }

    internal sealed class BoundAssignmentExpr : BoundExpr
    {
        public BoundAssignmentExpr(VariableSymbol variable, BoundExpr value)
        {
            Variable = variable;
            Value = value;
        }

        public VariableSymbol Variable { get; }
        public BoundExpr Value { get; }
        public override Type Type => Variable.Type;
        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpr;
    }
}
