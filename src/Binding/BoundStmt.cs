using Stonylang.Symbols;
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
        public BoundGoToStmt(BoundLabel label) => Label = label;
        public override BoundNodeKind Kind => BoundNodeKind.GoToStatement;
        public BoundLabel Label { get; }
    }

    internal sealed class BoundConditionalGoToStmt : BoundStmt
    {
        public BoundConditionalGoToStmt(BoundLabel label, BoundExpr condition, bool jumpIfTrue = true)
        {
            Label = label;
            Condition = condition;
            JumpIfTrue = jumpIfTrue;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGoToStatement;
        public BoundLabel Label { get; }
        public BoundExpr Condition { get; }
        public bool JumpIfTrue { get; }
    }

    internal sealed class BoundLabelStmt : BoundStmt
    {
        public BoundLabelStmt(BoundLabel label) => Label = label;
        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
        public BoundLabel Label { get; }
    }
}
