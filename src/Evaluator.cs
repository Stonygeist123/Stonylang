using Stonylang.Binding;
using Stonylang.Builtin;
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
        private Random _random;

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
                            double @fOperand => u.Op.OpKind switch
                            {
                                BoundUnaryOpKind.Identity => fOperand,
                                BoundUnaryOpKind.Negation => -fOperand,
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
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op.Kind}> for type {b.Left.Type}.")
                                },
                                double @rfO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => liO + rfO,
                                    BoundBinaryOpKind.Subtraction => liO - rfO,
                                    BoundBinaryOpKind.Multiplication => liO * rfO,
                                    BoundBinaryOpKind.Division => liO / rfO,
                                    BoundBinaryOpKind.Power => (int)Math.Pow(liO, rfO),
                                    BoundBinaryOpKind.Modulo => liO % rfO,
                                    BoundBinaryOpKind.LogicalEq => liO == rfO,
                                    BoundBinaryOpKind.LogicalNotEq => liO != rfO,
                                    BoundBinaryOpKind.Greater => liO > rfO,
                                    BoundBinaryOpKind.GreaterEq => liO >= rfO,
                                    BoundBinaryOpKind.Less => liO < rfO,
                                    BoundBinaryOpKind.LessEq => liO <= rfO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op.Kind}> for type {b.Left.Type}.")
                                },
                                string @rsO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => Stringify(liO) + rsO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
                                },
                                _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}."),
                            },

                            // left is float (double)
                            double @lfO => right switch
                            {
                                int @riO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => lfO + riO,
                                    BoundBinaryOpKind.Subtraction => lfO - riO,
                                    BoundBinaryOpKind.Multiplication => lfO * riO,
                                    BoundBinaryOpKind.Division => lfO / riO,
                                    BoundBinaryOpKind.Power => Math.Pow(lfO, riO),
                                    BoundBinaryOpKind.Greater => lfO > riO,
                                    BoundBinaryOpKind.GreaterEq => lfO >= riO,
                                    BoundBinaryOpKind.Less => lfO < riO,
                                    BoundBinaryOpKind.LessEq => lfO <= riO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
                                },
                                double @rfO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => lfO + rfO,
                                    BoundBinaryOpKind.Subtraction => lfO - rfO,
                                    BoundBinaryOpKind.Multiplication => lfO * rfO,
                                    BoundBinaryOpKind.Division => lfO / rfO,
                                    BoundBinaryOpKind.Power => Math.Pow(lfO, rfO),
                                    BoundBinaryOpKind.LogicalEq => lfO == rfO,
                                    BoundBinaryOpKind.LogicalNotEq => lfO != rfO,
                                    BoundBinaryOpKind.Greater => lfO > rfO,
                                    BoundBinaryOpKind.GreaterEq => lfO >= rfO,
                                    BoundBinaryOpKind.Less => lfO < rfO,
                                    BoundBinaryOpKind.LessEq => lfO <= rfO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
                                },
                                string @rsO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => Stringify(lfO) + rsO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
                                },
                                _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
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
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
                                },
                                string @rsO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => Stringify(lbO) + rsO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
                                },
                                _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}."),
                            },

                            // left is string
                            string @lsO => right switch
                            {
                                string @rsO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => lsO + rsO,
                                    BoundBinaryOpKind.LogicalEq => lsO == rsO,
                                    BoundBinaryOpKind.LogicalNotEq => lsO != rsO,
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
                                },
                                int @riO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => lsO + Stringify(riO),
                                    BoundBinaryOpKind.Subtraction => riO == Math.Abs(riO) ? lsO[..^riO] : lsO + Math.Abs(riO),
                                    BoundBinaryOpKind.Multiplication => string.Concat(Enumerable.Repeat(lsO, riO)),
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
                                },
                                double @rfO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => lsO + Stringify(rfO),
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
                                },
                                bool @rbO => b.Op.OpKind switch
                                {
                                    BoundBinaryOpKind.Addition => lsO + Stringify(rbO),
                                    _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
                                },
                                _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}."),
                            },
                            _ => throw new Exception($"Unexpected binary operator <{b.Op}> for type {b.Left.Type}.")
                        };
                    }

                case BoundCallExpr c:
                    {
                        // TODO: Distinguish between print and println
                        if (c.Function == BuiltinFunctions.Input)
                            return Console.ReadLine();
                        else if (c.Function == BuiltinFunctions.Print)
                            Console.WriteLine(Stringify(EvaluateExpression(c.Arguments[0])));
                        else if (c.Function == BuiltinFunctions.PrintLn)
                            Console.WriteLine(Stringify(EvaluateExpression(c.Arguments[0])));
                        else if (c.Function == BuiltinFunctions.Stringify)
                            return Stringify(EvaluateExpression(c.Arguments[0]));
                        else if (c.Function == BuiltinFunctions.RandomInt || c.Function == BuiltinFunctions.RandomFloat)
                        {
                            object v = EvaluateExpression(c.Arguments[0]);
                            if (_random == null)
                                _random = new();
                            return v is int i ? _random.Next(i) : _random.NextDouble() * (double)EvaluateExpression(c.Arguments[0]);
                        }
                        else
                            throw new Exception($"Unexpected function {c.Function.Name}");
                        break;
                    }

                case BoundConversionExpr c:
                    {
                        object v = EvaluateExpression(c.Expr);
                        if (c.Type == TypeSymbol.Bool)
                            return Convert.ToBoolean(v);
                        else if (c.Type == TypeSymbol.Int)
                            return Convert.ToInt32(v);
                        else if (c.Type == TypeSymbol.Float)
                            return Convert.ToDouble(v);
                        else if (c.Type == TypeSymbol.String)
                            return Stringify(v);
                        else
                            throw new Exception($"Unexpected type {c.Type}");
                    }

                default:
                    throw new Exception($"Unexpected node {node.Kind}.");
            }

            return null;
        }

        public static string Stringify(object v)
        {
            if (v is bool b)
                return b ? "true" : "false";
            if (v is double d)
                return d.ToString().Replace(',', '.');
            return v.ToString();
        }
    }
}
