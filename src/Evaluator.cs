using Stonylang_CSharp.Binding;
using Stonylang_CSharp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stonylang_CSharp.Evaluator
{
    public sealed class EvaluationResult
    {
        public EvaluationResult(DiagnosticBag diagnostics, object value)
        {
            Diagnostics = diagnostics;
            Value = value;
        }

        public DiagnosticBag Diagnostics { get; }
        public object Value { get; }
    }

    internal sealed class Evaluator
    {
        private object _lastValue;
        private readonly BoundStmt _root;
        private readonly Dictionary<string, VariableSymbol> _symbolTable;

        public Evaluator(BoundStmt root, Dictionary<string, VariableSymbol> symbolTable)
        {
            _root = root;
            _symbolTable = symbolTable;
        }

        public object Evaluate()
        {
            EvaluateStatement(_root);
            return _lastValue;
        }

        private void EvaluateStatement(BoundStmt node)
        {
            switch (node)
            {
                case BoundBlockStmt b:
                    foreach (BoundStmt stmt in b.Statements) EvaluateStatement(stmt);
                    break;
                case BoundExpressionStmt e:
                    _lastValue = EvaluateExpression(e.Expression);
                    break;
                case BoundVariableStmt v:
                    EvaluateVariableStmt(v);
                    break;
                default:
                    throw new Exception($"Unexpected node {node.Kind}.");
            }
        }

        private void EvaluateVariableStmt(BoundVariableStmt v)
        {
            object value = EvaluateExpression(v.Initializer);
            if (_symbolTable.ContainsKey(v.Variable.Name))
                _symbolTable[v.Variable.Name] = new(v.Variable.Name, _symbolTable[v.Variable.Name].Type, value, _symbolTable[v.Variable.Name].Span);
            else
                _symbolTable.Add(v.Variable.Name, new(v.Variable.Name, v.Variable.Type, value, v.Variable.Span));
            _lastValue = value;
        }

        private object EvaluateExpression(BoundExpr node)
        {
            switch (node)
            {
                case BoundLiteralExpr l:
                    return l.Value;
                case BoundVariableExpr v:
                    return _symbolTable[v.Variable.Name].Value;
                case BoundAssignmentExpr a:
                    {
                        object value = EvaluateExpression(a.Value);
                        _symbolTable[a.Variable.Name] = new(a.Variable.Name, _symbolTable[a.Variable.Name].Type, value, _symbolTable[a.Variable.Name].Span);
                        return value;
                    }

                case BoundUnaryExpr u:
                    {
                        object operand = EvaluateExpression(u.Operand);
                        if (operand is int @iOperand)
                            return u.Op.OpKind switch
                            {
                                BoundUnaryOpKind.Identity => iOperand,
                                BoundUnaryOpKind.Negation => -iOperand,
                                BoundUnaryOpKind.LogicalNegation => Enumerable.Range(1, iOperand).Aggregate(1, (p, i) => p * i),
                                BoundUnaryOpKind.Inv => ~iOperand,
                                _ => throw new Exception($"Unexpected unary operator <{u.Op}>.")
                            };

                        if (operand is bool @bOperand)
                            return u.Op.OpKind switch
                            {
                                BoundUnaryOpKind.LogicalNegation => !bOperand,
                                _ => throw new Exception($"Unexpected unary operator <{u.Op}>.")
                            };

                        break;
                    }

                case BoundBinaryExpr b:
                    {
                        object left = EvaluateExpression(b.Left);
                        object right = EvaluateExpression(b.Right);

                        if (left is int @liO && right is int @riO)
                            return b.Op.OpKind switch
                            {
                                BoundBinaryOpKind.Addition => liO + riO,
                                BoundBinaryOpKind.Subtraction => liO - riO,
                                BoundBinaryOpKind.Multiplication => liO * riO,
                                BoundBinaryOpKind.Division => liO / riO,
                                BoundBinaryOpKind.Power => (int)Math.Pow(liO, riO),
                                BoundBinaryOpKind.Modulo => liO % riO,
                                BoundBinaryOpKind.And => liO & riO,
                                BoundBinaryOpKind.Or => liO | riO,
                                BoundBinaryOpKind.LogicalEq => liO == riO,
                                BoundBinaryOpKind.LogicalNotEq => liO != riO,
                                BoundBinaryOpKind.Greater => liO > riO,
                                BoundBinaryOpKind.GreaterEq => liO >= riO,
                                BoundBinaryOpKind.Less => liO < riO,
                                BoundBinaryOpKind.LessEq => liO <= riO,
                                BoundBinaryOpKind.Rsh => liO >> riO,
                                BoundBinaryOpKind.Lsh => liO << riO,
                                BoundBinaryOpKind.Xor => liO ^ riO,
                                _ => throw new Exception($"Unexpected binary operator <{b.Op.Kind}> for type {left.GetType()}.")
                            };

                        if (left is bool @lbO && right is bool @rbO)
                            return b.Op.OpKind switch
                            {
                                BoundBinaryOpKind.LogicalAnd => lbO && rbO,
                                BoundBinaryOpKind.LogicalOr => lbO || rbO,
                                BoundBinaryOpKind.LogicalEq => lbO == rbO,
                                BoundBinaryOpKind.LogicalNotEq => lbO != rbO,
                                BoundBinaryOpKind.And => lbO & rbO,
                                BoundBinaryOpKind.Or => lbO | rbO,
                                BoundBinaryOpKind.Xor => lbO ^ rbO,
                                _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}.")
                            };

                        break;
                    }
            }

            throw new Exception($"Unexpected node {node.Kind}.");
        }
    }
}
