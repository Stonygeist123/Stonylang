using System;
using System.Collections.Immutable;

namespace Stonylang.Binding
{
    internal abstract class BoundTreeRewriter
    {
        public virtual BoundStmt RewriteStmt(BoundStmt node) => node.Kind switch
        {
            BoundNodeKind.BlockStatement => RewriteBlockStmt((BoundBlockStmt)node),
            BoundNodeKind.VariableStatement => RewriteVariableStmt((BoundVariableStmt)node),
            BoundNodeKind.IfStatement => RewriteIfStmt((BoundIfStmt)node),
            BoundNodeKind.WhileStatement => RewriteWhileStmt((BoundWhileStmt)node),
            BoundNodeKind.ForStatement => RewriteForStmt((BoundForStmt)node),
            BoundNodeKind.LabelStatement => RewriteLabelStatement((BoundLabelStmt)node),
            BoundNodeKind.GoToStatement => RewriteGoToStatement((BoundGoToStmt)node),
            BoundNodeKind.ConditionalGoToStatement => RewriteConditionalGoToStmt((BoundConditionalGoToStmt)node),
            BoundNodeKind.ExpressionStatement => RewriteExpressionStmt((BoundExpressionStmt)node),
            _ => throw new Exception($"Unexpected node: {node.Kind}."),
        };

        public virtual BoundExpr RewriteExpr(BoundExpr node) => node.Kind switch
        {
            BoundNodeKind.ErrorExpr => RewriteErrorExpr((BoundErrorExpr)node),
            BoundNodeKind.LiteralExpr => RewriteLiteralExpr((BoundLiteralExpr)node),
            BoundNodeKind.UnaryExpr => RewriteUnaryExpr((BoundUnaryExpr)node),
            BoundNodeKind.BinaryExpr => RewriteBinaryExpr((BoundBinaryExpr)node),
            BoundNodeKind.VariableExpr => RewriteVariablExpr((BoundVariableExpr)node),
            BoundNodeKind.AssignmentExpr => RewriteAssignmentExpr((BoundAssignmentExpr)node),
            BoundNodeKind.CallExpr => RewriteCallExpr((BoundCallExpr)node),
            BoundNodeKind.ConversionExpr => RewriteConversionExpr((BoundConversionExpr)node),
            _ => throw new Exception($"Unexpected node: {node.Kind}."),
        };

        protected virtual BoundStmt RewriteBlockStmt(BoundBlockStmt node)
        {
            ImmutableArray<BoundStmt>.Builder builder = null;
            for (int i = 0; i < node.Statements.Length; ++i)
            {
                BoundStmt oldStmt = node.Statements[i];
                BoundStmt newStmt = RewriteStmt(oldStmt);
                if (oldStmt != newStmt)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundStmt>(node.Statements.Length);
                        for (int j = 0; j < i; ++j)
                            builder.Add(node.Statements[j]);
                    }
                }
                if (builder != null)
                    builder.Add(newStmt);
            }

            if (builder == null)
                return node;
            return new BoundBlockStmt(builder.MoveToImmutable());
        }

        protected virtual BoundStmt RewriteVariableStmt(BoundVariableStmt node)
        {
            BoundExpr initializer = RewriteExpr(node.Initializer);
            if (initializer == node.Initializer)
                return node;
            return new BoundVariableStmt(node.Variable, initializer);
        }

        protected virtual BoundStmt RewriteIfStmt(BoundIfStmt node)
        {
            BoundExpr condition = RewriteExpr(node.Condition);
            BoundBlockStmt thenBranch = (BoundBlockStmt)RewriteBlockStmt(node.ThenBranch);
            BoundBlockStmt elseBranch = node.ElseBranch == null ? null : (BoundBlockStmt)RewriteBlockStmt(node.ElseBranch);

            if (condition == node.Condition && thenBranch == node.ThenBranch && elseBranch == node.ElseBranch)
                return node;
            return new BoundIfStmt(condition, thenBranch, elseBranch);
        }

        protected virtual BoundStmt RewriteWhileStmt(BoundWhileStmt node)
        {
            BoundExpr condition = RewriteExpr(node.Condition);
            BoundBlockStmt stmt = (BoundBlockStmt)RewriteStmt(node.Stmt);

            if (condition == node.Condition && stmt == node.Stmt)
                return node;
            return new BoundWhileStmt(condition, stmt, node.IsDoWhile);
        }

        protected virtual BoundStmt RewriteForStmt(BoundForStmt node)
        {
            BoundExpr initialValue = RewriteExpr(node.InitialValue);
            BoundExpr range = RewriteExpr(node.Range);
            BoundBlockStmt stmt = (BoundBlockStmt)RewriteStmt(node.Stmt);

            if (initialValue == node.InitialValue && range == node.Range && stmt == node.Stmt)
                return node;
            return new BoundForStmt(node.Variable, initialValue, range, stmt);
        }

        protected virtual BoundStmt RewriteLabelStatement(BoundLabelStmt node) => node;

        protected virtual BoundStmt RewriteGoToStatement(BoundGoToStmt node) => node;

        protected virtual BoundStmt RewriteConditionalGoToStmt(BoundConditionalGoToStmt node)
        {
            BoundExpr condition = RewriteExpr(node.Condition);
            if (condition == node.Condition)
                return node;
            return new BoundConditionalGoToStmt(node.Label, condition, node.JumpIfTrue);
        }

        protected virtual BoundStmt RewriteExpressionStmt(BoundExpressionStmt node)
        {
            BoundExpr expr = RewriteExpr(node.Expression);
            if (expr == node.Expression)
                return node;
            return new BoundExpressionStmt(node.Expression);
        }

        protected virtual BoundExpr RewriteErrorExpr(BoundErrorExpr node) => node;

        protected virtual BoundExpr RewriteLiteralExpr(BoundLiteralExpr node) => node;

        protected virtual BoundExpr RewriteVariablExpr(BoundVariableExpr node) => node;

        protected virtual BoundExpr RewriteAssignmentExpr(BoundAssignmentExpr node)
        {
            BoundExpr expr = RewriteExpr(node.Value);
            if (expr == node.Value)
                return node;
            return new BoundAssignmentExpr(node.Variable, node.Value);
        }

        protected virtual BoundExpr RewriteUnaryExpr(BoundUnaryExpr node)
        {
            BoundExpr expr = RewriteExpr(node.Operand);
            if (expr == node.Operand)
                return node;
            return new BoundUnaryExpr(node.Op, node.Operand);
        }

        protected virtual BoundExpr RewriteBinaryExpr(BoundBinaryExpr node)
        {
            BoundExpr left = RewriteExpr(node.Left);
            BoundExpr right = RewriteExpr(node.Right);
            if (left == node.Left && right == node.Right)
                return node;
            return new BoundBinaryExpr(left, node.Op, right);
        }

        protected virtual BoundExpr RewriteCallExpr(BoundCallExpr node)
        {
            ImmutableArray<BoundExpr>.Builder builder = null;
            for (int i = 0; i < node.Arguments.Length; ++i)
            {
                BoundExpr oldExpr = node.Arguments[i];
                BoundExpr newExpr = RewriteExpr(oldExpr);
                if (oldExpr != newExpr)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundExpr>(node.Arguments.Length);
                        for (int j = 0; j < i; ++j)
                            builder.Add(node.Arguments[j]);
                    }
                }
                if (builder != null)
                    builder.Add(newExpr);
            }

            if (builder == null)
                return node;

            return new BoundCallExpr(node.Function, builder.ToImmutable());
        }

        protected virtual BoundExpr RewriteConversionExpr(BoundConversionExpr node)
        {
            BoundExpr expr = RewriteExpr(node.Expr);
            if (expr == node.Expr)
                return node;

            return new BoundConversionExpr(node.Type, expr);
        }
    }
}
