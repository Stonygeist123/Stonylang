﻿using Stonylang_CSharp.Binding;
using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Stonylang_CSharp.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount;
        private Lowerer() { }
        public static BoundBlockStmt Lower(BoundStmt stmt) => Flatten(new Lowerer().RewriteStmt(stmt));
        private LabelSymbol GenerateLabel() => new($"Label{++_labelCount}");

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
                LabelSymbol endLabel = GenerateLabel();
                BoundConditionalGoToStmt gotoFalse = new(endLabel, node.Condition, true);
                BoundLabelStmt endLabelStmt = new(endLabel);
                BoundBlockStmt result = new(ImmutableArray.Create<BoundStmt>(gotoFalse, node.ThenBranch, endLabelStmt));
                return RewriteStmt(result);
            }
            else
            {
                LabelSymbol elseLabel = GenerateLabel();
                LabelSymbol endLabel = GenerateLabel();

                BoundConditionalGoToStmt gotoFalse = new(elseLabel, node.Condition, true);
                BoundGoToStmt gotoEndStmt = new(endLabel);
                BoundLabelStmt elseLabelStmt = new(elseLabel);
                BoundLabelStmt endLabelStmt = new(endLabel);
                BoundBlockStmt result = new(ImmutableArray.Create<BoundStmt>(gotoFalse, node.ThenBranch, gotoEndStmt, elseLabelStmt, node.ElseBranch, endLabelStmt));
                return RewriteStmt(result);
            }
        }

        protected override BoundStmt RewriteWhileStmt(BoundWhileStmt node)
        {
            LabelSymbol continueLabel = GenerateLabel();
            LabelSymbol checkLabel = GenerateLabel();
            LabelSymbol endLabel = GenerateLabel();

            BoundGoToStmt gotoCheck = new(checkLabel);
            BoundLabelStmt continueLabelStmt = new(continueLabel);
            BoundLabelStmt checkLabelStmt = new(checkLabel);
            BoundConditionalGoToStmt gotoTrue = new(continueLabel, node.Condition, false);
            BoundLabelStmt endLabelStmt = new(endLabel);
            BoundBlockStmt result = new(ImmutableArray.Create<BoundStmt>(gotoCheck, continueLabelStmt, node.Stmt, checkLabelStmt, gotoTrue, endLabelStmt));
            return RewriteStmt(result);
        }

        protected override BoundStmt RewriteForStmt(BoundForStmt node)
        {
            BoundVariableStmt variableDecl = new(node.Variable, node.InitialValue);
            BoundVariableExpr variableExpr = new(node.Variable);

            BoundBinaryExpr condition = new(variableExpr, BoundBinaryOperator.Bind(SyntaxKind.LessEq, typeof(int), typeof(int)), node.Range);
            BoundExpressionStmt increment = new(new BoundAssignmentExpr(node.Variable,
                new BoundBinaryExpr(variableExpr, BoundBinaryOperator.Bind(SyntaxKind.Plus, typeof(int), typeof(int)), new BoundLiteralExpr(1))));

            BoundBlockStmt whileBlock = new(ImmutableArray.Create<BoundStmt>(node.Stmt, increment));
            BoundWhileStmt whileStmt = new(condition, whileBlock, false);
            BoundBlockStmt result = new(ImmutableArray.Create<BoundStmt>(variableDecl, whileStmt));
            return RewriteStmt(result);
        }
    }
}
