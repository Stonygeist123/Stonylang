using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Stonylang_CSharp.Parser
{
    public abstract class Node
    {
        public virtual TextSpan Span => TextSpan.FromRounds(GetChildren().First().Span.Start, GetChildren().Last().Span.End);
        public abstract SyntaxKind Kind { get; }
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
}
