using Stonylang.Binding;
using Stonylang.Symbols;
using Stonylang.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stonylang.Evaluator
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
        private readonly BoundBlockStmt _root;
        private readonly Dictionary<string, VariableSymbol> _symbolTable;

        public Evaluator(BoundBlockStmt root, Dictionary<string, VariableSymbol> symbolTable)
        {
            _root = root;
            _symbolTable = symbolTable;
        }

        public object Evaluate()
        {
            Dictionary<BoundLabel, int> labelToIndex = new();
            for (int i = 0; i < _root.Statements.Length; ++i)
                if (_root.Statements[i] is BoundLabelStmt l)
                    labelToIndex.Add(l.Label, i + 1);

            int index = 0;
            while (index < _root.Statements.Length)
            {
                BoundStmt s = _root.Statements[index];
                switch (s.Kind)
                {
                    case BoundNodeKind.VariableStatement:
                        EvaluateVariableStmt((BoundVariableStmt)s);
                        ++index;
                        break;
                    case BoundNodeKind.ExpressionStatement:
                        _lastValue = EvaluateExpression(((BoundExpressionStmt)s).Expression);
                        ++index;
                        break;
                    case BoundNodeKind.GoToStatement:
                        BoundGoToStmt gs = (BoundGoToStmt)s;
                        index = labelToIndex[gs.Label];
                        break;
                    case BoundNodeKind.ConditionalGoToStatement:
                        BoundConditionalGoToStmt cgs = (BoundConditionalGoToStmt)s;
                        bool condition = (bool)EvaluateExpression(cgs.Condition);
                        if (condition == cgs.JumpIfTrue)
                            index = labelToIndex[cgs.Label];
                        else
                            ++index;
                        break;
                    case BoundNodeKind.LabelStatement:
                        ++index;
                        break;
                    default:
                        throw new Exception($"Unexpected node {s.Kind}.");
                }
            }
            return _lastValue;
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
                        return operand switch
                        {
                            int @iOperand => u.Op.OpKind switch
                            {
                                BoundUnaryOpKind.Identity => iOperand,
                                BoundUnaryOpKind.Negation => -iOperand,
                                BoundUnaryOpKind.LogicalNegation => Enumerable.Range(1, iOperand).Aggregate(1, (p, i) => p * i),
                                BoundUnaryOpKind.Inv => ~iOperand,
                                _ => throw new Exception($"Unexpected unary operator <{u.Op}>.")
                            },
                            bool @bOperand => u.Op.OpKind switch
                            {
                                BoundUnaryOpKind.LogicalNegation => !bOperand,
                                _ => throw new Exception($"Unexpected unary operator <{u.Op}>.")
                            },
                            _ => throw new Exception($"Unexpected unary operator <{u.Op}>."),
                        };
                    }

                case BoundBinaryExpr b:
                    {
                        object left = EvaluateExpression(b.Left);
                        object right = EvaluateExpression(b.Right);

                        return left switch
                        {
                            // left is int
                            int @liO => right switch
                            {
                                int @riO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => liO + riO,
                                    BoundBinaryOpKind.Subtraction => liO - riO,
                                    BoundBinaryOpKind.Multiplication => liO * riO,
                                    BoundBinaryOpKind.Division => liO / riO,
                                    BoundBinaryOpKind.Power => (int)Math.Pow(liO, riO),
                                    BoundBinaryOpKind.Modulo => liO % riO,
                                    BoundBinaryOpKind.LogicalEq => liO == riO,
                                    BoundBinaryOpKind.LogicalNotEq => liO != riO,
                                    BoundBinaryOpKind.Greater => liO > riO,
                                    BoundBinaryOpKind.GreaterEq => liO >= riO,
                                    BoundBinaryOpKind.Less => liO < riO,
                                    BoundBinaryOpKind.LessEq => liO <= riO,
                                    BoundBinaryOpKind.Rsh => liO >> riO,
                                    BoundBinaryOpKind.Lsh => liO << riO,
                                    BoundBinaryOpKind.BitwiseAnd => liO & riO,
                                    BoundBinaryOpKind.BitwiseOr => liO | riO,
                                    BoundBinaryOpKind.BitwiseXor => liO ^ riO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op.Kind}> for type {left.GetType()}.")
                                },
                                string @rsO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => liO + rsO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}.")
                                },
                                _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}."),
                            },

                            // left is bool
                            bool @lbO => right switch
                            {
                                bool @rbO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.LogicalAnd => lbO && rbO,
                                    BoundBinaryOpKind.LogicalOr => lbO || rbO,
                                    BoundBinaryOpKind.LogicalEq => lbO == rbO,
                                    BoundBinaryOpKind.LogicalNotEq => lbO != rbO,
                                    BoundBinaryOpKind.BitwiseAnd => lbO & rbO,
                                    BoundBinaryOpKind.BitwiseOr => lbO | rbO,
                                    BoundBinaryOpKind.BitwiseXor => lbO ^ rbO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}.")
                                },
                                string @rsO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => lbO + rsO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}.")
                                },
                                _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}."),
                            },

                            // left is string
                            string @lsO => right switch
                            {
                                string @rsO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => lsO + rsO,
                                    BoundBinaryOpKind.LogicalEq => lsO == rsO,
                                    BoundBinaryOpKind.LogicalNotEq => lsO != rsO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}.")
                                },
                                int @riO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => lsO + riO,
                                    BoundBinaryOpKind.Subtraction => riO == Math.Abs(riO) ? lsO.Substring(0, lsO.Length - riO) : lsO + Math.Abs(riO),
                                    BoundBinaryOpKind.Multiplication => string.Concat(Enumerable.Repeat(lsO, riO)),
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}.")
                                },
                                bool @rbO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => lsO + (rbO ? "true" : "false"),
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}.")
                                },
                                _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}."),
                            },
                            _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {left.GetType()}."),
                        };
                    }
            }

            throw new Exception($"Unexpected node {node.Kind}.");
        }
    }
}
