using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Parser;
using System.Collections.Generic;
using System.Linq;

namespace Stonylang_CSharp.SyntaxTree
{
    class SyntaxTree
    {
        public SyntaxTree(IEnumerable<string> diagnostics, ExprNode root, Token eofToken)
        {
            Diagnostics = diagnostics.ToArray();
            Root = root;
            EofToken = eofToken;
        }

        public IReadOnlyList<string> Diagnostics { get; }
        public ExprNode Root { get; }
        public Token EofToken { get; }

        public static SyntaxTree Parse(string source) => new Parser.Parser(source).Parse();
    }
}
