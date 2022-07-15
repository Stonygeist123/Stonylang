using Stonylang.Symbols;
using System;
using System.Collections.Immutable;

namespace Stonylang.Binding
{
    internal enum BoundNodeKind
    {
        // Exprs
        LiteralExpr,

        UnaryExpr,
        BinaryExpr,
        VariableExpr,
        AssignmentExpr,
        CallExpr,
        ConversionExpr,
        ErrorExpr,

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
        public abstract TypeSymbol Type { get; }
    }

    internal sealed class BoundLiteralExpr : BoundExpr
    {
        public BoundLiteralExpr(object value)
        {
            Value = value;
            Type = value switch
            {
                bool => TypeSymbol.Bool,
                int => TypeSymbol.Int,
                double => TypeSymbol.Float,
                string => TypeSymbol.String,
                _ => throw new Exception($"Unexpected literal \"{value}\" of type {value.GetType()}.")
            };
        }

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpr;
        public override TypeSymbol Type { get; }
        public object Value { get; }
    }

    internal sealed class BoundUnaryExpr : BoundExpr
    {
        public BoundUnaryExpr(BoundUnaryOperator op, BoundExpr operand)
        {
            Op = op;
            Operand = operand;
        }

        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpr;
        public override TypeSymbol Type => Op.ResultType;
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

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpr;
        public override TypeSymbol Type => Op.ResultType;
        public BoundExpr Left { get; }
        public BoundBinaryOperator Op { get; }
        public BoundExpr Right { get; }
    }

    internal sealed class BoundVariableExpr : BoundExpr
    {
        public BoundVariableExpr(VariableSymbol variable) => Variable = variable;
        public override BoundNodeKind Kind => BoundNodeKind.VariableExpr;
        public override TypeSymbol Type => Variable.Type;
        public VariableSymbol Variable { get; }
    }

    internal sealed class BoundAssignmentExpr : BoundExpr
    {
        public BoundAssignmentExpr(VariableSymbol variable, BoundExpr value)
        {
            Variable = variable;
            Value = value;
        }

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpr;
        public override TypeSymbol Type => Variable.Type;
        public VariableSymbol Variable { get; }
        public BoundExpr Value { get; }
    }

    internal sealed class BoundCallExpr : BoundExpr
    {
        public BoundCallExpr(FunctionSymbol function, ImmutableArray<BoundExpr> arguments)
        {
            Function = function;
            Arguments = arguments;
        }

        public override BoundNodeKind Kind => BoundNodeKind.CallExpr;
        public override TypeSymbol Type => Function.Type;
        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpr> Arguments { get; }
    }

    internal sealed class BoundConversionExpr : BoundExpr
    {
        public BoundConversionExpr(TypeSymbol type, BoundExpr expr)
        {
            Type = type;
            Expr = expr;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConversionExpr;
        public override TypeSymbol Type { get; }
        public BoundExpr Expr { get; }
    }

    internal sealed class BoundErrorExpr : BoundExpr
    {
        public override BoundNodeKind Kind => BoundNodeKind.ErrorExpr;
        public override TypeSymbol Type => TypeSymbol.Error;
    }
}
