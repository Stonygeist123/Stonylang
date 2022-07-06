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
        private readonly BoundExpr _root;
        private readonly Dictionary<string, VariableSymbol> _symbolTable;

        public Evaluator(BoundExpr root, Dictionary<string, VariableSymbol> symbolTable)
        {
            _root = root;
            _symbolTable = symbolTable;
        }

        public object Evaluate() => EvaluateExpression(_root);
        private object EvaluateExpression(BoundExpr node)
        {
            if (node is BoundLiteralExpr l) return l.Value;
            else if (node is BoundVariableExpr v) return _symbolTable[v.Variable.Name].Value;
            else if (node is BoundAssignmentExpr a)
            {
                object value = EvaluateExpression(a.Value);
                if (_symbolTable.ContainsKey(a.Variable.Name))
                    _symbolTable[a.Variable.Name] = new(a.Variable.Name, _symbolTable[a.Variable.Name].Type, value, _symbolTable[a.Variable.Name].Span);
                else _symbolTable.Add(a.Variable.Name, new(a.Variable.Name, a.Type, value, a.Variable.Span));
                return value;
            }
            else if (node is BoundUnaryExpr u)
            {
                object operand = EvaluateExpression(u.Operand);
                if (operand is int @iOperand)
                {
                    return u.Op.OpKind switch
                    {
                        BoundUnaryOpKind.Identity => iOperand,
                        BoundUnaryOpKind.Negation => -iOperand,
                        BoundUnaryOpKind.LogicalNegation => Enumerable.Range(1, iOperand).Aggregate(1, (p, i) => p * i),
                        BoundUnaryOpKind.Inv => ~iOperand,
                        _ => throw new Exception($"Unexpected unary operator <{u.Op}>.")
                    };
                }
                if (operand is bool @bOperand)
                {
                    return u.Op.OpKind switch
                    {
                        BoundUnaryOpKind.LogicalNegation => !bOperand,
                        _ => throw new Exception($"Unexpected unary operator <{u.Op}>.")
                    };
                }
            }
            else if (node is BoundBinaryExpr b)
            {
                object left = EvaluateExpression(b.Left);
                object right = EvaluateExpression(b.Right);

                if (left is int @liO && right is int @riO)
                {
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
                        _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}.")
                    };
                }

                if (left is bool @lbO && right is bool @rbO)
                {
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
                }
            }

            throw new Exception($"Unexpected node {node.Kind}.");
        }
    }
}
