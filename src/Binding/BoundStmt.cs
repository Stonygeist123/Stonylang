using Stonylang_CSharp.Utility;
using System.Collections.Immutable;

namespace Stonylang_CSharp.Binding
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
        public BoundExpr Expression { get; }
        public VariableSymbol Variable { get; }
        public BoundExpr Initializer { get; }
    }
}
