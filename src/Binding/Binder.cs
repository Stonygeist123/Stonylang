using Stonylang.Builtin;
using Stonylang.Lexer;
using Stonylang.Parser;
using Stonylang.Symbols;
using Stonylang.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Stonylang.Binding
{
    internal sealed class BoundUnaryOperator
    {
        private BoundUnaryOperator(SyntaxKind kind, BoundUnaryOpKind opKind, TypeSymbol operandType)
            : this(kind, opKind, operandType, operandType) { }

        private BoundUnaryOperator(SyntaxKind kind, BoundUnaryOpKind opKind, TypeSymbol operandType, TypeSymbol resultType)
        {
            Kind = kind;
            OpKind = opKind;
            OperandType = operandType;
            ResultType = resultType;
        }

        public SyntaxKind Kind { get; }
        public BoundUnaryOpKind OpKind { get; }
        public TypeSymbol OperandType { get; }
        public TypeSymbol ResultType { get; }

        private static readonly BoundUnaryOperator[] _operators =
        {
            new(SyntaxKind.Not, BoundUnaryOpKind.LogicalNegation, TypeSymbol.Bool),
            new(SyntaxKind.Not, BoundUnaryOpKind.LogicalNegation, TypeSymbol.Int),
            new(SyntaxKind.Plus, BoundUnaryOpKind.Identity, TypeSymbol.Int),
            new(SyntaxKind.Minus, BoundUnaryOpKind.Negation, TypeSymbol.Int),
            new(SyntaxKind.Plus, BoundUnaryOpKind.Identity, TypeSymbol.Float),
            new(SyntaxKind.Minus, BoundUnaryOpKind.Negation, TypeSymbol.Float),
            new(SyntaxKind.Inv, BoundUnaryOpKind.Inv, TypeSymbol.Int),
            new(SyntaxKind.Increment, BoundUnaryOpKind.Increment, TypeSymbol.Int),
            new(SyntaxKind.Decrement, BoundUnaryOpKind.Decrement, TypeSymbol.Int)
        };

        public static BoundUnaryOperator Bind(SyntaxKind kind, TypeSymbol operandType)
        {
            foreach (BoundUnaryOperator op in _operators)
                if (op.Kind == kind && op.OperandType == operandType)
                    return op;
            return null;
        }
    }

    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind kind, BoundBinaryOpKind opKind, TypeSymbol type)
            : this(kind, opKind, type, type, type) { }

        private BoundBinaryOperator(SyntaxKind kind, BoundBinaryOpKind opKind, TypeSymbol operandType, TypeSymbol type)
            : this(kind, opKind, operandType, operandType, type) { }

        private BoundBinaryOperator(SyntaxKind kind, BoundBinaryOpKind opKind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
        {
            Kind = kind;
            OpKind = opKind;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        public SyntaxKind Kind { get; }
        public BoundBinaryOpKind OpKind { get; }
        public TypeSymbol LeftType { get; }
        public TypeSymbol RightType { get; }
        public TypeSymbol ResultType { get; }

        private static readonly BoundBinaryOperator[] _operators =
        {
            // bool
            new(SyntaxKind.LogicalAnd, BoundBinaryOpKind.LogicalAnd, TypeSymbol.Bool),
            new(SyntaxKind.LogicalOr, BoundBinaryOpKind.LogicalOr, TypeSymbol.Bool),
            new(SyntaxKind.EqEq, BoundBinaryOpKind.LogicalEq, TypeSymbol.Bool),
            new(SyntaxKind.NotEq, BoundBinaryOpKind.LogicalNotEq, TypeSymbol.Bool),
            new(SyntaxKind.Or, BoundBinaryOpKind.BitwiseOr, TypeSymbol.Bool),
            new(SyntaxKind.And, BoundBinaryOpKind.BitwiseAnd, TypeSymbol.Bool),
            new(SyntaxKind.Xor, BoundBinaryOpKind.BitwiseXor, TypeSymbol.Bool),

            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, TypeSymbol.Bool, TypeSymbol.String),

            // int
            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, TypeSymbol.Int),
            new(SyntaxKind.Minus, BoundBinaryOpKind.Subtraction, TypeSymbol.Int),
            new(SyntaxKind.Star, BoundBinaryOpKind.Multiplication, TypeSymbol.Int),
            new(SyntaxKind.Slash, BoundBinaryOpKind.Division, TypeSymbol.Int),
            new(SyntaxKind.Power, BoundBinaryOpKind.Power, TypeSymbol.Int),
            new(SyntaxKind.Mod, BoundBinaryOpKind.Modulo, TypeSymbol.Int),
            new(SyntaxKind.Or, BoundBinaryOpKind.BitwiseOr, TypeSymbol.Int),
            new(SyntaxKind.And, BoundBinaryOpKind.BitwiseAnd, TypeSymbol.Int),
            new(SyntaxKind.Xor, BoundBinaryOpKind.BitwiseXor, TypeSymbol.Int),
            new(SyntaxKind.Rsh, BoundBinaryOpKind.Rsh, TypeSymbol.Int),
            new(SyntaxKind.Lsh, BoundBinaryOpKind.Lsh, TypeSymbol.Int),

            new(SyntaxKind.EqEq, BoundBinaryOpKind.LogicalEq, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.NotEq, BoundBinaryOpKind.LogicalNotEq, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.Greater, BoundBinaryOpKind.Greater, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.GreaterEq, BoundBinaryOpKind.GreaterEq, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.Less, BoundBinaryOpKind.Less, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.LessEq, BoundBinaryOpKind.LessEq, TypeSymbol.Int, TypeSymbol.Bool),

            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Float),
            new(SyntaxKind.Minus, BoundBinaryOpKind.Subtraction, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Float),
            new(SyntaxKind.Star, BoundBinaryOpKind.Multiplication, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Float),
            new(SyntaxKind.Slash, BoundBinaryOpKind.Division, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Float),
            new(SyntaxKind.Power, BoundBinaryOpKind.Power, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Float),

            new(SyntaxKind.Greater, BoundBinaryOpKind.Greater, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Bool),
            new(SyntaxKind.GreaterEq, BoundBinaryOpKind.GreaterEq, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Bool),
            new(SyntaxKind.Less, BoundBinaryOpKind.Less, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Bool),
            new(SyntaxKind.LessEq, BoundBinaryOpKind.LessEq, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Bool),

            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, TypeSymbol.Int, TypeSymbol.String, TypeSymbol.String),

            // float
            
            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, TypeSymbol.Float),
            new(SyntaxKind.Minus, BoundBinaryOpKind.Subtraction, TypeSymbol.Float),
            new(SyntaxKind.Star, BoundBinaryOpKind.Multiplication, TypeSymbol.Float),
            new(SyntaxKind.Slash, BoundBinaryOpKind.Division, TypeSymbol.Float),
            new(SyntaxKind.Power, BoundBinaryOpKind.Power, TypeSymbol.Float),

            new(SyntaxKind.EqEq, BoundBinaryOpKind.LogicalEq, TypeSymbol.Float, TypeSymbol.Bool),
            new(SyntaxKind.NotEq, BoundBinaryOpKind.LogicalNotEq, TypeSymbol.Float, TypeSymbol.Bool),
            new(SyntaxKind.Greater, BoundBinaryOpKind.Greater, TypeSymbol.Float, TypeSymbol.Bool),
            new(SyntaxKind.GreaterEq, BoundBinaryOpKind.GreaterEq, TypeSymbol.Float, TypeSymbol.Bool),
            new(SyntaxKind.Less, BoundBinaryOpKind.Less, TypeSymbol.Float, TypeSymbol.Bool),
            new(SyntaxKind.LessEq, BoundBinaryOpKind.LessEq, TypeSymbol.Float, TypeSymbol.Bool),

            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Float),
            new(SyntaxKind.Minus, BoundBinaryOpKind.Subtraction, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Float),
            new(SyntaxKind.Star, BoundBinaryOpKind.Multiplication, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Float),
            new(SyntaxKind.Slash, BoundBinaryOpKind.Division, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Float),
            new(SyntaxKind.Power, BoundBinaryOpKind.Power, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Float),

            new(SyntaxKind.Greater, BoundBinaryOpKind.Greater, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.GreaterEq, BoundBinaryOpKind.GreaterEq, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.Less, BoundBinaryOpKind.Less, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Bool),
            new(SyntaxKind.LessEq, BoundBinaryOpKind.LessEq, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Bool),

            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, TypeSymbol.Float, TypeSymbol.String, TypeSymbol.String),

            // string
            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, TypeSymbol.String),
            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, TypeSymbol.String, TypeSymbol.Bool, TypeSymbol.String),
            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, TypeSymbol.String, TypeSymbol.Int, TypeSymbol.String),
            new(SyntaxKind.Plus, BoundBinaryOpKind.Addition, TypeSymbol.String, TypeSymbol.Float, TypeSymbol.String),
            new(SyntaxKind.Star, BoundBinaryOpKind.Multiplication, TypeSymbol.String, TypeSymbol.Int, TypeSymbol.String),
            new(SyntaxKind.Minus, BoundBinaryOpKind.Subtraction, TypeSymbol.String, TypeSymbol.Int, TypeSymbol.String),

            new(SyntaxKind.EqEq, BoundBinaryOpKind.LogicalEq, TypeSymbol.String, TypeSymbol.Bool),
            new(SyntaxKind.NotEq, BoundBinaryOpKind.LogicalNotEq, TypeSymbol.String, TypeSymbol.Bool)
        };

        public static BoundBinaryOperator Bind(SyntaxKind kind, TypeSymbol leftType, TypeSymbol rightType)
        {
            foreach (var op in _operators)
                if (op.Kind == kind && op.LeftType == leftType && op.RightType == rightType)
                    return op;
            return null;
        }
    }

    internal sealed partial class Binder
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

            BoundScope parent = CreateRootScope();
            while (stack.Count > 0)
            {
                previous = stack.Pop();
                BoundScope scope = new(parent);
                foreach (var v in previous.Variables) scope.TryDeclareVariable(v);

                parent = scope;
            }

            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            BoundScope result = new(null);
            foreach (FunctionSymbol f in BuiltinFunctions.GetAll())
                result.TryDeclareFunction(f);
            return result;
        }

        public BoundStmt BindStatement(StmtNode stmt) => stmt.Kind switch
        {
            SyntaxKind.BlockStmt => BindBlockStmt((BlockStmt)stmt),
            SyntaxKind.VariableStmt => BindVariableStmt((VariableStmt)stmt),
            SyntaxKind.IfStmt => BindIfStmt((IfStmt)stmt),
            SyntaxKind.WhileStmt => BindWhileStmt((WhileStmt)stmt),
            SyntaxKind.ForStmt => BindForStmt((ForStmt)stmt),
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
            TypeSymbol type = stmt.Type == null ? initializer.Type : LookUpType(stmt.Type.Lexeme);
            if (type == null)
            {
                _diagnostics.Report(_source, stmt.Type.Span, $"Unknown type \"{stmt.Type.Lexeme}\".", "TypeException", LogLevel.Error);
                type = TypeSymbol.Error;
            }
            initializer = BindConversion(stmt.Initializer, type, false);
            return new(BindVariable(stmt.Identifier, type, stmt.IsMut), initializer);
        }

        private BoundIfStmt BindIfStmt(IfStmt stmt)
        {
            BoundExpr condition = BindExpression(stmt.Condition, TypeSymbol.Bool);
            return new(condition, BindBlockStmt(stmt.ThenBranch), stmt.ElseBranch == null ? null : BindBlockStmt(stmt.ElseBranch.ElseBranch));
        }

        private BoundWhileStmt BindWhileStmt(WhileStmt stmt)
        {
            BoundExpr condition = stmt.Condition == null ? new BoundLiteralExpr(true) : BindExpression(stmt.Condition, TypeSymbol.Bool);
            return new(condition, BindBlockStmt(stmt.ThenBranch), stmt.DoKeyword != null);
        }

        private BoundForStmt BindForStmt(ForStmt stmt)
        {
            BoundExpr initialValue = BindExpression(stmt.InitialValue, TypeSymbol.Int);
            BoundExpr range = BindExpression(stmt.Range, TypeSymbol.Int);
            _scope = new BoundScope(_scope);

            VariableSymbol variable = BindVariable(stmt.Identifier, TypeSymbol.Int, stmt.IsMut);
            BoundBlockStmt body = BindBlockStmt(stmt.Stmt);

            _scope = _scope.Parent;
            return new(variable, initialValue, range, body);
        }

        private BoundExpressionStmt BindExpressionStmt(ExpressionStmt stmt) => new(BindExpression(stmt.Expression, true));

        public BoundExpr BindExpression(ExprNode expr, bool canBeVoid = false)
        {
            BoundExpr boundExpr = BindExpressionInternal(expr);
            if (!canBeVoid && boundExpr.Type == TypeSymbol.Void)
            {
                _diagnostics.Report(_source, expr.Span, $"Expression must have a non-void value.", "TypeException", LogLevel.Error);
                return new BoundErrorExpr();
            }

            return boundExpr;
        }

        public BoundExpr BindExpression(ExprNode expr, TypeSymbol expectedType) => BindConversion(expr, expectedType);
        public BoundExpr BindExpressionInternal(ExprNode expr) => expr.Kind switch
        {
            SyntaxKind.LiteralExpr => BindLiteralExpr((LiteralExpr)expr),
            SyntaxKind.UnaryExpr => BindUnaryExpr((UnaryExpr)expr),
            SyntaxKind.BinaryExpr => BindBinaryExpr((BinaryExpr)expr),
            SyntaxKind.NameExpr => BindNameExpr((NameExpr)expr),
            SyntaxKind.AssignmentExpr => BindAssignmentExpr((AssignmentExpr)expr),
            SyntaxKind.CallExpr => BindCallExpr((CallExpr)expr),
            _ => throw new Exception($"Unexpected syntax \"{expr.Kind}\"."),
        };

        private static BoundExpr BindLiteralExpr(LiteralExpr expr) => new BoundLiteralExpr(expr.Value ?? 0);

        private BoundExpr BindUnaryExpr(UnaryExpr expr)
        {
            BoundExpr boundOperand = BindExpression(expr.Operand);
            if (boundOperand.Type == TypeSymbol.Error)
                return new BoundErrorExpr();

            BoundUnaryOperator boundOperator = BoundUnaryOperator.Bind(expr.Op.Kind, boundOperand.Type);
            if (boundOperator != null)
            {
                if (boundOperator.OpKind == BoundUnaryOpKind.Increment || boundOperator.OpKind == BoundUnaryOpKind.Decrement)
                {
                    if (boundOperand is BoundVariableExpr bv)
                    {
                        if (_scope.TryLookUpVariable(bv.Variable.Name, out var variable))
                        {
                            if (!variable.IsMut)
                                _diagnostics.Report(_source, expr.Op.Span, $"Cannot assign to \"{bv.Variable.Name}\" since it is a read-only variable.", "AssignmentExcption", LogLevel.Error);
                            if (variable.Type != TypeSymbol.Int)
                                _diagnostics.Report(_source, expr.Op.Span, $"Cannot assign type of \"int\" to {bv.Variable.Name}, which has a type of {bv.Variable.Type}.", "TypeException", LogLevel.Error);
                            return new BoundAssignmentExpr(bv.Variable, new BoundBinaryExpr(new BoundVariableExpr(variable), BoundBinaryOperator.Bind(boundOperator.Kind == SyntaxKind.Increment ? SyntaxKind.Plus : SyntaxKind.Minus, TypeSymbol.Int, TypeSymbol.Int), new BoundLiteralExpr(1)));
                        }
                        _diagnostics.Report(_source, expr.Op.Span, $"Could not find \"{bv.Variable.Name}\" in the current context.", "KeyNotFoundException", LogLevel.Error);
                    }
                    _diagnostics.Report(_source, expr.Op.Span,
                        $"The operand of an {(boundOperator.OpKind == BoundUnaryOpKind.Increment ? "increment" : "decrement")} operator must be a variable with type of \"int\".\nGot \"{boundOperand.Type}\"", "AssignmentException", LogLevel.Error);
                }
                return new BoundUnaryExpr(boundOperator, boundOperand);
            }

            _diagnostics.Report(_source, expr.Op.Span, $"Unary operator '{expr.Op.Lexeme}' is not defined for type \"{boundOperand.Type}\".", "TypeException", LogLevel.Error);
            return new BoundErrorExpr();
        }

        private BoundExpr BindBinaryExpr(BinaryExpr expr)
        {
            BoundExpr boundLeft = BindExpression(expr.Left);
            BoundExpr boundRight = BindExpression(expr.Right);
            if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
                return new BoundErrorExpr();

            BoundBinaryOperator boundOperator = BoundBinaryOperator.Bind(expr.Op.Kind, boundLeft.Type, boundRight.Type);
            if (boundOperator != null)
                return new BoundBinaryExpr(boundLeft, boundOperator, boundRight);

            _diagnostics.Report(_source, expr.Op.Span, $"Binary operator '{expr.Op.Lexeme}' is not defined for types \"{boundLeft.Type}\" and \"{boundRight.Type}\".", "TypeException", LogLevel.Error);
            return new BoundErrorExpr();
        }

        private BoundExpr BindNameExpr(NameExpr expr)
        {
            if (expr.Name.IsMissing)
                return new BoundErrorExpr();

            if (_scope.TryLookUpVariable(expr.Name.Lexeme, out var value))
                return new BoundVariableExpr(value);

            if (expr.Name.Kind != SyntaxKind.EOF)
                _diagnostics.Report(_source, expr.Name.Span, $"Could not find \"{expr.Name.Lexeme}\" in the current context.", "KeyNotFoundException", LogLevel.Error);
            return new BoundErrorExpr();
        }

        private BoundExpr BindAssignmentExpr(AssignmentExpr expr)
        {
            string name = expr.Name.Lexeme;
            BoundExpr boundExpresion = BindExpression(expr.Value);

            if (!_scope.TryLookUpVariable(name, out var variable))
            {
                if (expr.Name.Kind != SyntaxKind.EOF)
                    _diagnostics.Report(_source, expr.Name.Span, $"Could not find \"{name}\" in the current context.", "KeyNotFoundException", LogLevel.Error);
                return new BoundErrorExpr();
            }

            if (!variable.IsMut)
                _diagnostics.Report(_source, expr.Name.Span, $"Cannot assign to \"{name}\" since it is a read-only variable.", "AssignmentException", LogLevel.Error);

            BoundExpr boundConversion = BindConversion(expr.Value, variable.Type, false);

            return new BoundAssignmentExpr(new(name, boundExpresion.Type, null, expr.EqualsToken.Span), boundConversion);
        }

        private BoundExpr BindCallExpr(CallExpr expr)
        {
            if (expr.Arguments.Count == 1 && LookUpType(expr.Identifier.Lexeme) is TypeSymbol type)
                return BindConversion(expr.Arguments[0], type, true);

            IEnumerable<FunctionSymbol> functions = BuiltinFunctions.GetAll();
            if (!_scope.TryLookUpFunction(expr.Identifier.Lexeme, out var function))
            {
                _diagnostics.Report(_source, expr.Identifier.Span, $"Function \"{expr.Identifier.Lexeme}\" doesn't exist in the current context.", "KeyNotFoundException", LogLevel.Error);
                return new BoundErrorExpr();
            }

            ImmutableArray<BoundExpr> boundArguments = (from i in expr.Arguments
                                                        select BindExpression(i)).ToImmutableArray();

            if (boundArguments.Length != function.Parameters.Length)
            {
                for (int i = 0; i < function.Parameters.Length; i++)
                {
                    if (boundArguments.ElementAtOrDefault(i) == null || function.Parameters[i].Type != boundArguments[i].Type)
                        _diagnostics.Report(_source, expr.Arguments.LastOrDefault<Node>(expr.LParen).Span,
                            $"There is no given argument that corresponds to the required formal parameter \"{function.Parameters[i].Name}\" of \"{function.Name}\".", "ArgumentException", LogLevel.Error);
                }

                if (boundArguments.Length > function.Parameters.Length)
                    _diagnostics.Report(_source, expr.Arguments.LastOrDefault<Node>(expr.LParen).Span,
                        $"Expected {function.Parameters.Length} arguments, but got {boundArguments.Length}.", "ArgumentException", LogLevel.Error);

                return new BoundErrorExpr();
            }

            return new BoundCallExpr(function, boundArguments);
        }

        private BoundExpr BindConversion(ExprNode expr, TypeSymbol type, bool allowExplicit = false)
        {
            BoundExpr boundExpr = BindExpression(expr);
            Conversion conversion = Conversion.Classify(boundExpr.Type, type);
            if (!conversion.Exists)
            {
                if (boundExpr.Type != TypeSymbol.Error &&
                    type != TypeSymbol.Error &&
                    boundExpr.Type != type)
                    _diagnostics.Report(_source, expr.Span,
                        $"Cannot convert a value of type \"{boundExpr.Type}\" to a value of type \"{type}\".", "TypeException", LogLevel.Error);
                return new BoundErrorExpr();
            }

            if (!allowExplicit && conversion.IsExplicit)
                _diagnostics.Report(_source, expr.Span,
                    $"Cannot implicitly convert a value of type \"{boundExpr.Type}\" to a value of type \"{type}\". An explicit conversion exists; are you missing a cast?", "TypeException", LogLevel.Error);

            /*if (conversion.IsIdentity)
            {
                if (conversionInfo)
                    _diagnostics.Report(_source, expr.Span, $"Unnecessary conversion from type \"{type}\" to \"{type}\"", "TypeInfo", LogLevel.Info);
                return boundExpr;
            }*/
            return new BoundConversionExpr(type, boundExpr);
        }

        private VariableSymbol BindVariable(Token identifier, TypeSymbol type, bool isMut)
        {
            VariableSymbol variable = new(identifier.Lexeme ?? "?", type, null, identifier.Span, isMut);
            if (!identifier.IsMissing && !_scope.TryDeclareVariable(variable) || _scope.TryLookUpFunction(identifier.Lexeme, out _))
                _diagnostics.Report(_source, identifier.Span, $"\"{identifier.Lexeme}\" was already declared in the current or a previous scope.", "DeclarationException", LogLevel.Error);
            return variable;
        }

        private static TypeSymbol LookUpType(string name) => name switch
        {
            "bool" => TypeSymbol.Bool,
            "int" => TypeSymbol.Int,
            "float" => TypeSymbol.Float,
            "string" => TypeSymbol.String,
            _ => null,
        };
    }
}
