using Stonylang.Binding;
using Stonylang.Lexer;
using Stonylang.Symbols;
using Stonylang.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Stonylang.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount;

        private Lowerer()
        { }

        public static BoundBlockStmt Lower(BoundStmt stmt) => Flatten(new Lowerer().RewriteStmt(stmt));

        private BoundLabel GenerateLabel() => new($"Label{++_labelCount}");

        private static BoundBlockStmt Flatten(BoundStmt stmt)
        {
            ImmutableArray<BoundStmt>.Builder builder = ImmutableArray.CreateBuilder<BoundStmt>();
            Stack<BoundStmt> stack = new();
            stack.Push(stmt);

            while (stack.Count > 0)
            {
                BoundStmt current = stack.Pop();
                if (current is BoundBlockStmt block)
                    foreach (BoundStmt s in block.Statements.Reverse())
                        stack.Push(s);
                else
                    builder.Add(current);
            }
            return new(builder.ToImmutable());
        }

        protected override BoundStmt RewriteIfStmt(BoundIfStmt node)
        {
            if (node.ElseBranch == null)
            {
                BoundLabel endLabel = GenerateLabel();
                BoundConditionalGoToStmt gotoFalse = new(endLabel, node.Condition, false);
                BoundLabelStmt endLabelStmt = new(endLabel);
                BoundBlockStmt result = new(ImmutableArray.Create<BoundStmt>(
                    gotoFalse,
                    node.ThenBranch,
                    endLabelStmt
                ));
                return RewriteStmt(result);
            }
            else
            {
                BoundLabel elseLabel = GenerateLabel();
                BoundLabel endLabel = GenerateLabel();

                BoundConditionalGoToStmt gotoFalse = new(elseLabel, node.Condition, false);
                BoundGoToStmt gotoEndStmt = new(endLabel);
                BoundLabelStmt elseLabelStmt = new(elseLabel);
                BoundLabelStmt endLabelStmt = new(endLabel);
                BoundBlockStmt result = new(ImmutableArray.Create<BoundStmt>(
                    gotoFalse,
                    node.ThenBranch,
                    gotoEndStmt,
                    elseLabelStmt,
                    node.ElseBranch,
                    endLabelStmt
                ));
                return RewriteStmt(result);
            }
        }

        protected override BoundStmt RewriteWhileStmt(BoundWhileStmt node)
        {
            if (node.IsDoWhile)
            {
                BoundLabel continueLabel = GenerateLabel();
                BoundLabelStmt continueLabelStmt = new(continueLabel);
                BoundConditionalGoToStmt gotoTrue = new(continueLabel, node.Condition);
                BoundBlockStmt result = new(ImmutableArray.Create<BoundStmt>(
                    continueLabelStmt,
                    node.Stmt,
                    gotoTrue
                ));

                return RewriteStmt(result);
            }
            else
            {
                BoundLabel continueLabel = GenerateLabel();
                BoundLabel checkLabel = GenerateLabel();
                BoundLabel endLabel = GenerateLabel();

                BoundGoToStmt gotoCheck = new(checkLabel);
                BoundLabelStmt continueLabelStmt = new(continueLabel);
                BoundLabelStmt checkLabelStmt = new(checkLabel);
                BoundConditionalGoToStmt gotoTrue = new(continueLabel, node.Condition);
                BoundLabelStmt endLabelStmt = new(endLabel);
                BoundBlockStmt result = new(ImmutableArray.Create<BoundStmt>(
                    gotoCheck,
                    continueLabelStmt,
                    node.Stmt,
                    checkLabelStmt,
                    gotoTrue,
                    endLabelStmt
                ));
                return RewriteStmt(result);
            }
        }

        protected override BoundStmt RewriteForStmt(BoundForStmt node)
        {
            BoundVariableStmt variableDecl = new(node.Variable, node.InitialValue);
            BoundVariableExpr variableExpr = new(node.Variable);
            VariableSymbol upperBoundSymbol = new("upperBound", TypeSymbol.Int, null, null, true);
            BoundVariableStmt upperBoundDecl = new(upperBoundSymbol, node.Range);

            BoundBinaryExpr condition = new(variableExpr, BoundBinaryOperator.Bind(SyntaxKind.Less, TypeSymbol.Int, TypeSymbol.Int), new BoundVariableExpr(upperBoundSymbol));
            BoundExpressionStmt increment = new(new BoundAssignmentExpr(node.Variable,
                new BoundBinaryExpr(variableExpr, BoundBinaryOperator.Bind(SyntaxKind.Plus, TypeSymbol.Int, TypeSymbol.Int), new BoundLiteralExpr(1))));

            BoundBlockStmt whileBody = new(ImmutableArray.Create<BoundStmt>(node.Stmt, increment));
            BoundWhileStmt whileStmt = new(condition, whileBody, false);
            BoundBlockStmt result = new(ImmutableArray.Create<BoundStmt>(
                variableDecl,
                upperBoundDecl,
                whileStmt
            ));
            return RewriteStmt(result);
        }
    }
}
