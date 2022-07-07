using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Parser;
using Stonylang_CSharp.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stonylang_CSharp.Binding
{
    internal sealed class BoundUnaryOperator
    {
        private BoundUnaryOperator(SyntaxKind kind, BoundUnaryOpKind opKind, Type operandType)
            : this(kind, opKind, operandType, operandType) { }
        private BoundUnaryOperator(SyntaxKind kind, BoundUnaryOpKind opKind, Type operandType, Type resultType)
        {
            Kind = kind;
            OpKind = opKind;
            OperandType = operandType;
            ResultType = resultType;
        }

        public SyntaxKind Kind { get; }
        public BoundUnaryOpKind OpKind { get; }
        public Type OperandType { get; }
        public Type ResultType { get; }

        private static readonly BoundUnaryOperator[] _operators =
        {
            new(SyntaxKind.Not, BoundUnaryOpKind.LogicalNegation, typeof(bool)),
            new(SyntaxKind.Not, BoundUnaryOpKind.LogicalNegation, typeof(int)),
            new(SyntaxKind.Plus, BoundUnaryOpKind.Identity, typeof(int)),
            new(SyntaxKind.Minus, BoundUnaryOpKind.Negation, typeof(int)),
            new(SyntaxKind.Inv, BoundUnaryOpKind.Inv, typeof(int)),
            new(SyntaxKind.Increment, BoundUnaryOpKind.Increment, typeof(int)),
            new(SyntaxKind.Decrement, BoundUnaryOpKind.Decrement, typeof(int))
        };

        public static BoundUnaryOperator Bind(SyntaxKind kind, Type operandType)
        {
            foreach (var op in _operators)
                if (op.Kind == kind && op.OperandType == operandType) return op;
            return null;
        }
    }

    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind kind, BoundBinaryOpKind opKind, Type type)
            : this(kind, opKind, type, type, type) { }
        private BoundBinaryOperator(SyntaxKind kind, BoundBinaryOpKind opKind, Type operandType, Type type)
            : this(kind, opKind, operandType, operandType, type) { }
        private BoundBinaryOperator(SyntaxKind kind, BoundBinaryOpKind opKind, Type leftType, Type rightType, Type resultType)
        {
            Kind = kind;
            OpKind = opKind;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        public SyntaxKind Kind { get; }
        public BoundBinaryOpKind OpKind { get; }
        public Type LeftType { get; }
        public Type RightType { get; }
        public Type ResultType { get; }

        private static readonly BoundBinaryOperator[] _operators =
        {
            // int
            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, typeof(int)),
            new(SyntaxKind.Minus, BoundBinaryOpKind.Subtraction, typeof(int)),
            new(SyntaxKind.Star, BoundBinaryOpKind.Multiplication, typeof(int)),
            new(SyntaxKind.Slash, BoundBinaryOpKind.Division, typeof(int)),
            new(SyntaxKind.Power, BoundBinaryOpKind.Power, typeof(int)),
            new(SyntaxKind.Mod, BoundBinaryOpKind.Modulo, typeof(int)),
            new(SyntaxKind.Or, BoundBinaryOpKind.Or, typeof(int)),
            new(SyntaxKind.And, BoundBinaryOpKind.And, typeof(int)),
            new(SyntaxKind.Xor, BoundBinaryOpKind.Xor, typeof(int)),
            new(SyntaxKind.Rsh, BoundBinaryOpKind.Rsh, typeof(int)),
            new(SyntaxKind.Lsh, BoundBinaryOpKind.Lsh, typeof(int)),

            new(SyntaxKind.EqEq, BoundBinaryOpKind.LogicalEq, typeof(int), typeof(bool)),
            new(SyntaxKind.NotEq, BoundBinaryOpKind.LogicalNotEq, typeof(int), typeof(bool)),
            new(SyntaxKind.Greater, BoundBinaryOpKind.Greater, typeof(int), typeof(bool)),
            new(SyntaxKind.GreaterEq, BoundBinaryOpKind.GreaterEq, typeof(int), typeof(bool)),
            new(SyntaxKind.Less, BoundBinaryOpKind.Less, typeof(int), typeof(bool)),
            new(SyntaxKind.LessEq, BoundBinaryOpKind.LessEq, typeof(int), typeof(bool)),

            // bool
            new(SyntaxKind.LogicalAnd, BoundBinaryOpKind.LogicalAnd, typeof(bool)),
            new(SyntaxKind.LogicalOr, BoundBinaryOpKind.LogicalOr, typeof(bool)),
            new(SyntaxKind.EqEq, BoundBinaryOpKind.LogicalEq, typeof(bool)),
            new(SyntaxKind.NotEq, BoundBinaryOpKind.LogicalNotEq, typeof(bool)),
            new(SyntaxKind.Or, BoundBinaryOpKind.Or, typeof(bool)),
            new(SyntaxKind.And, BoundBinaryOpKind.And, typeof(bool)),
            new(SyntaxKind.Xor, BoundBinaryOpKind.Xor, typeof(bool))
        };

        public static BoundBinaryOperator Bind(SyntaxKind kind, Type leftType, Type rightType)
        {
            foreach (var op in _operators)
                if (op.Kind == kind && op.LeftType == leftType && op.RightType == rightType) return op;
            return null;
        }
    }

    internal sealed class Binder
    {
        private readonly DiagnosticBag _diagnostics = new();
        private readonly SourceText _source;
        private BoundScope _scope;
        public DiagnosticBag Diagnostics => _diagnostics;
        public Binder(SourceText source, BoundScope parentScope)
        {
            _scope = new(parentScope);
            _source = source;
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, SourceText source, CompilationUnitSyntax syntax)
        {
            BoundScope parentScope = CreateParentScope(previous);
            Binder binder = new(source, parentScope);
            BoundStmt stmt = binder.BindStatement(syntax.Statement);
            ImmutableArray<VariableSymbol> variables = binder._scope.GetDeclaredVariables();
            DiagnosticBag diagnostics = binder.Diagnostics;

            if (previous != null) diagnostics = diagnostics.AddRange(previous.Diagnostics);
            return new(previous, diagnostics, variables, stmt);
        }

        private static BoundScope CreateParentScope(BoundGlobalScope previous)
        {
            Stack<BoundGlobalScope> stack = new();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            BoundScope parent = null;
            while (stack.Count > 0)
            {
                previous = stack.Pop();
                BoundScope scope = new(parent);
                foreach (var v in previous.Variables) scope.TryDeclare(v, out var _);

                parent = scope;
            }

            return parent;
        }

        public BoundStmt BindStatement(StmtNode stmt) => stmt.Kind switch
        {
            SyntaxKind.BlockStmt => BindBlockStmt((BlockStmt)stmt),
            SyntaxKind.VariableStmt => BindVariableStmt((VariableStmt)stmt),
            SyntaxKind.ExpressionStmt => BindExpressionStmt((ExpressionStmt)stmt),
            _ => throw new Exception($"Unexpected syntax \"{stmt.Kind}\"."),
        };

        private BoundBlockStmt BindBlockStmt(BlockStmt stmt)
        {
            ImmutableArray<BoundStmt>.Builder statements = ImmutableArray.CreateBuilder<BoundStmt>();
            _scope = new(_scope);

            foreach (StmtNode statement in stmt.Statements)
                statements.Add(BindStatement(statement));

            _scope = _scope.Parent;
            return new(statements.ToImmutable());
        }

        private BoundVariableStmt BindVariableStmt(VariableStmt stmt)
        {
            BoundExpr initializer = BindExpression(stmt.Initializer);
            VariableSymbol variable = new(stmt.Identifier.Lexeme, initializer.Type, null, stmt.Identifier.Span, stmt.IsMut);

            if (!_scope.TryDeclare(variable, out _))
                _diagnostics.Report(_source, stmt.Identifier.Span, $"Variable \"{stmt.Identifier.Lexeme}\" was already declared in the current or a previous scope.", "DeclarationException", LogLevel.Error);

            /* if (!_scope.TryLookUp(variable.Name, out _))
                _diagnostics.Report(_source, stmt.Identifier.Span, $"Variable \"{stmt.Identifier.Lexeme}\" was already declared in a previous scope.", "DeclarationWarning", LogLevel.Warn); */

            return new(variable, initializer);
        }

        private BoundExpressionStmt BindExpressionStmt(ExpressionStmt stmt) => new(BindExpression(stmt.Expression));

        public BoundExpr BindExpression(ExprNode expr) => expr.Kind switch
        {
            SyntaxKind.LiteralExpr => BindLiteralExpr((LiteralExpr)expr),
            SyntaxKind.UnaryExpr => BindUnaryExpr((UnaryExpr)expr),
            SyntaxKind.BinaryExpr => BindBinaryExpr((BinaryExpr)expr),
            SyntaxKind.NameExpr => BindNameExpr((NameExpr)expr),
            SyntaxKind.AssignmentExpr => BindAssignmentExpr((AssignmentExpr)expr),
            _ => throw new Exception($"Unexpected syntax \"{expr.Kind}\"."),
        };

        private static BoundExpr BindLiteralExpr(LiteralExpr expr) => new BoundLiteralExpr(expr.Value ?? 0);
        private BoundExpr BindUnaryExpr(UnaryExpr expr)
        {
            BoundExpr boundOperand = BindExpression(expr.Operand);
            BoundUnaryOperator boundOperator = BoundUnaryOperator.Bind(expr.Op.Kind, boundOperand.Type);
            if (boundOperator != null)
            {
                if (boundOperator.OpKind == BoundUnaryOpKind.Increment || boundOperator.OpKind == BoundUnaryOpKind.Decrement)
                {
                    if (boundOperand is BoundVariableExpr bv)
                    {
                        if (_scope.TryLookUp(bv.Variable.Name, out var value))
                        {
                            if (value.Type != typeof(int))
                                _diagnostics.Report(_source, expr.Op.Span, $"Cannot assign type of \"int\" to {bv.Variable.Name}, which has a type of {bv.Variable.Type}", "TypeException", LogLevel.Error);
                            return new BoundAssignmentExpr(bv.Variable, new BoundBinaryExpr(new BoundVariableExpr(value), BoundBinaryOperator.Bind(boundOperator.Kind == SyntaxKind.Increment ? SyntaxKind.Plus : SyntaxKind.Minus, typeof(int), typeof(int)), new BoundLiteralExpr(1)));
                        }
                        _diagnostics.Report(_source, expr.Op.Span, $"Could not find \"{bv.Variable.Name}\" in the current context.", "KeyNotFoundException", LogLevel.Error);
                    }
                    _diagnostics.Report(_source, expr.Op.Span,
                        $"The operand of an {(boundOperator.OpKind == BoundUnaryOpKind.Increment ? "increment" : "decrement")} operator must be a variable with type of \"int\".\nGot \"{boundOperand.Type}\"", "AssignmentException", LogLevel.Error);
                }
                return new BoundUnaryExpr(boundOperator, boundOperand);
            }

            _diagnostics.Report(_source, expr.Op.Span, $"Unary operator '{expr.Op.Lexeme}' is not defined for type \"{boundOperand.Type}\".", "TypeException", LogLevel.Error);
            return boundOperand;
        }

        private BoundExpr BindBinaryExpr(BinaryExpr expr)
        {
            BoundExpr boundLeft = BindExpression(expr.Left);
            BoundExpr boundRight = BindExpression(expr.Right);
            BoundBinaryOperator boundOperator = BoundBinaryOperator.Bind(expr.Op.Kind, boundLeft.Type, boundRight.Type);
            if (boundOperator != null)
                return new BoundBinaryExpr(boundLeft, boundOperator, boundRight);

            _diagnostics.Report(_source, expr.Op.Span, $"Binary operator '{expr.Op.Lexeme}' is not defined for types \"{boundLeft.Type}\" and \"{boundRight.Type}\".", "TypeException", LogLevel.Error);
            return boundLeft;
        }

        private BoundExpr BindNameExpr(NameExpr expr)
        {
            string name = expr.Name.Lexeme;
            if (_scope.TryLookUp(name, out var value)) return new BoundVariableExpr(value);
            _diagnostics.Report(_source, expr.Name.Span, $"Could not find \"{name}\" in the current context.", "KeyNotFoundException", LogLevel.Error);
            return new BoundLiteralExpr(0);
        }

        private BoundExpr BindAssignmentExpr(AssignmentExpr expr)
        {
            string name = expr.Name.Lexeme;
            BoundExpr boundExpresion = BindExpression(expr.Value);

            if (!_scope.TryLookUp(name, out var variable))
            {
                _diagnostics.Report(_source, expr.Name.Span, $"Could not find \"{name}\" in the current context.", "KeyNotFoundException", LogLevel.Error);
                return new BoundLiteralExpr(0);
            }

            if (!variable.IsMut)
            {
                _diagnostics.Report(_source, expr.Name.Span, $"Cannot assign to \"{name}\" since it is a read-only variable.", "AssignmentException", LogLevel.Error);
                return new BoundLiteralExpr(0);
            }

            if (!variable.Type.Equals(boundExpresion.Type))
            {
                _diagnostics.Report(_source, expr.Name.Span, $"Cannot assign a type of \"{boundExpresion.Type}\" to \"{name}\", which has a type of \"{variable.Type}\".", "TypeException", LogLevel.Error);
                return boundExpresion;
            }

            return new BoundAssignmentExpr(new(name, boundExpresion.Type, null, expr.EqualsToken.Span), boundExpresion);
        }
    }
}
