using Stonylang.Utility;
using System.Collections.Immutable;

namespace Stonylang.Binding
{
    internal abstract class BoundStmt : BoundNode { }

    internal sealed class BoundBlockStmt : BoundStmt
    {
        public BoundBlockStmt(ImmutableArray<BoundStmt> statements) => Statements = statements;
        public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
        public ImmutableArray<BoundStmt> Statements { get; }
    }

    internal sealed class BoundExpressionStmt : BoundStmt
    {
        public BoundExpressionStmt(BoundExpr expression) => Expression = expression;
        public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;
        public BoundExpr Expression { get; }
    }

    internal sealed class BoundVariableStmt : BoundStmt
    {
        public BoundVariableStmt(VariableSymbol variable, BoundExpr initializer)
        {
            Variable = variable;
            Initializer = initializer;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableStatement;
        public VariableSymbol Variable { get; }
        public BoundExpr Initializer { get; }
    }

    internal sealed class BoundIfStmt : BoundStmt
    {
        public BoundIfStmt(BoundExpr condition, BoundBlockStmt thenBranch, BoundBlockStmt elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        public override BoundNodeKind Kind => BoundNodeKind.IfStatement;
        public BoundExpr Condition { get; }
        public BoundBlockStmt ThenBranch { get; }
        public BoundBlockStmt ElseBranch { get; }
    }

    internal sealed class BoundWhileStmt : BoundStmt
    {
        public BoundWhileStmt(BoundExpr condition, BoundBlockStmt stmt, bool isDoWhile)
        {
            Condition = condition;
            Stmt = stmt;
            IsDoWhile = isDoWhile;
        }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
        public BoundExpr Condition { get; }
        public BoundBlockStmt Stmt { get; }
        public bool IsDoWhile { get; }
    }

    internal sealed class BoundForStmt : BoundStmt
    {
        public BoundForStmt(VariableSymbol variable, BoundExpr initialValue, BoundExpr range, BoundBlockStmt stmt)
        {
            Variable = variable;
            InitialValue = initialValue;
            Range = range;
            Stmt = stmt;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ForStatement;
        public VariableSymbol Variable { get; }
        public BoundExpr InitialValue { get; }
        public BoundExpr Range { get; }
        public BoundBlockStmt Stmt { get; }
    }

    internal sealed class BoundGoToStmt : BoundStmt
    {
        public BoundGoToStmt(LabelSymbol label) => Label = label;
        public override BoundNodeKind Kind => BoundNodeKind.GoToStatement;
        public LabelSymbol Label { get; }
    }

    internal sealed class BoundConditionalGoToStmt : BoundStmt
    {
        public BoundConditionalGoToStmt(LabelSymbol label, BoundExpr condition, bool jumpIfTrue = false)
        {
            Label = label;
            Condition = condition;
            JumpIfTrue = jumpIfTrue;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGoToStatement;
        public LabelSymbol Label { get; }
        public BoundExpr Condition { get; }
        public bool JumpIfTrue { get; }
    }

    internal sealed class BoundLabelStmt : BoundStmt
    {
        public BoundLabelStmt(LabelSymbol label) => Label = label;
        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
        public LabelSymbol Label { get; }
    }
}
