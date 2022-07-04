using Stonylang_CSharp.Diagnostics;
using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Parser;
using System.Collections.Generic;
using System.Linq;

namespace Stonylang_CSharp.SyntaxTree
{
    public class SyntaxTree
    {
        public SyntaxTree(DiagnosticBag diagnostics, ExprNode root, Token eofToken)
        {
            Diagnostics = diagnostics;
            Root = root;
            EofToken = eofToken;
        }

        public DiagnosticBag Diagnostics { get; }
        public ExprNode Root { get; }
        public Token EofToken { get; }

        public static SyntaxTree Parse(string source) => new Parser.Parser(source).Parse();
    }
}
