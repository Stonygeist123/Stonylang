using Stonylang_CSharp.Utility;
using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.Parser;
using System.Collections.Generic;

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

        public static SyntaxTree Parse(SourceText source) => new Parser.Parser(source).Parse();
        public static SyntaxTree Parse(string source) => new Parser.Parser(SourceText.From(source)).Parse();
        public static IEnumerable<Token> ParseTokens(string source) => ParseTokens(SourceText.From(source));
        public static IEnumerable<Token> ParseTokens(SourceText source)
        {
            Lexer.Lexer lexer = new(source);
            while (true)
            {
                Token token = lexer.Lex();
                if (token.Kind == SyntaxKind.EOF) break;
                yield return token;
            }
        }
    }
}
