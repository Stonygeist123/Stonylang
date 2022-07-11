using Stonylang_CSharp.SyntaxFacts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Stonylang_CSharp.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
        public IEnumerable<BoundNode> GetChildren()
        {
            foreach (PropertyInfo prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (typeof(BoundNode).IsAssignableFrom(prop.PropertyType) && prop is not null)
                {
                    BoundNode child = (BoundNode)prop.GetValue(this);
                    if (child != null)
                        yield return child;
                }
                else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(prop.PropertyType))
                    foreach (BoundNode child in (IEnumerable<BoundNode>)prop.GetValue(this))
                        if (child != null)
                            yield return child;
        }

        private IEnumerable<(string Name, object Value)> GetProperties()
        {
            foreach (PropertyInfo prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.Name == nameof(Kind) || prop.Name == nameof(BoundBinaryExpr.Op))
                    continue;

                if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(prop.PropertyType) ||
                    typeof(IEnumerable<BoundNode>).IsAssignableFrom(prop.PropertyType))
                    continue;
                object value = prop.GetValue(this);
                if (value != null)
                    yield return (prop.Name, value);
            }
        }

        private static ConsoleColor GetColor(BoundNode node)
        {
            if (node is BoundExpr)
                return ConsoleColor.Blue;
            if (node is BoundStmt)
                return ConsoleColor.Cyan;
            return ConsoleColor.Yellow;
        }

        public void WriteTo(TextWriter writer) => PrettyPrint(writer, this);
        private static void PrettyPrint(TextWriter writer, BoundNode node, string indent = "", bool isLast = true)
        {
            bool isConsole = writer == Console.Out;
            string marker = isLast ? "└──" : "├──";

            if (isConsole)
                Console.ForegroundColor = ConsoleColor.DarkGray;

            writer.Write(indent);
            writer.Write(marker);

            if (isConsole)
                Console.ForegroundColor = GetColor(node);

            string text = GetText(node);
            writer.Write(text);

            bool isFirstProp = true;

            foreach (var (Name, Value) in node.GetProperties())
            {
                if (isFirstProp)
                    isFirstProp ^= true;
                else
                {
                    if (isConsole)
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    writer.Write(",");
                }

                writer.Write(" ");

                if (isConsole)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                writer.Write(Name);

                if (isConsole)
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                writer.Write(" = ");

                if (isConsole)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                writer.Write(Value);
            }

            if (isConsole)
                Console.ResetColor();

            writer.WriteLine();
            indent += isLast ? "   " : "|   ";
            BoundNode lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
                PrettyPrint(writer, child, indent, child == lastChild);
        }

        private static string GetText(BoundNode node) => node.Kind.ToString();
        public new string ToString()
        {
            using StringWriter writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }
    }
}
