using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Stonylang_CSharp.Parser
{
    public class Node
    {
        public virtual TextSpan Span => TextSpan.FromRounds(GetChildren().First().Span.Start, GetChildren().Last().Span.End);
        public virtual SyntaxKind Kind { get; }
        public IEnumerable<Node> GetChildren()
        {
            foreach (PropertyInfo prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (typeof(Node).IsAssignableFrom(prop.PropertyType))
                    yield return (Node)prop.GetValue(this);
                else if (typeof(IEnumerable<Node>).IsAssignableFrom(prop.PropertyType))
                    foreach (Node child in (IEnumerable<Node>)prop.GetValue(this))
                        yield return child;
        }

        public void WriteTo(TextWriter writer) => PrettyPrint(writer, this);
        private static void PrettyPrint(TextWriter writer, Node node, string indent = "", bool isLast = true)
        {
            string marker = isLast ? "└──" : "├──";
            writer.Write(indent);
            writer.Write(marker);
            writer.Write(node.Kind);

            if (node is Token @t && t.Literal != null)
            {
                writer.Write(" ");
                writer.Write(t.Literal);
            }
            writer.WriteLine();
            indent += isLast ? "   " : "|   ";
            Node lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren()) PrettyPrint(writer, child, indent, child == lastChild);
        }

        public new string ToString()
        {
            using StringWriter writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }
    }

    public abstract class ExprNode : Node
    {
        public override abstract SyntaxKind Kind { get; }
    }

    public sealed class LiteralExpr : ExprNode
    {
        public LiteralExpr(Token literalToken) : this(literalToken, literalToken.Literal) { }
        public LiteralExpr(Token literalToken, object value)
        {
            LiteralToken = literalToken;
            Value = value;
        }

        public override SyntaxKind Kind => SyntaxKind.LiteralExpr;
        public Token LiteralToken { get; }
        public object Value { get; }
    }

    public sealed class UnaryExpr : ExprNode
    {
        public UnaryExpr(Token op, ExprNode operand)
        {
            Operand = operand;
            Op = op;
        }

        public override SyntaxKind Kind => SyntaxKind.UnaryExpr;
        public Token Op { get; }
        public ExprNode Operand { get; }
    }

    public sealed class BinaryExpr : ExprNode
    {
        public BinaryExpr(ExprNode left, Token op, ExprNode right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpr;
        public ExprNode Left { get; }
        public Token Op { get; }
        public ExprNode Right { get; }
    }

    public sealed class NameExpr : ExprNode
    {
        public NameExpr(Token name) => Name = name;
        public override SyntaxKind Kind => SyntaxKind.NameExpr;
        public Token Name { get; }
    }

    public sealed class AssignmentExpr : ExprNode
    {
        public AssignmentExpr(Token name, Token equalsToken, ExprNode value)
        {
            Name = name;
            EqualsToken = equalsToken;
            Value = value;
        }

        public override SyntaxKind Kind => SyntaxKind.AssignmentExpr;
        public Token Name { get; }
        public Token EqualsToken { get; }
        public ExprNode Value { get; }
    }
}
