using Stonylang.Utility;
using Stonylang.Lexer;
using Stonylang.Parser;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Stonylang.SyntaxTree
{
    public class SyntaxTree
    {
        private SyntaxTree(SourceText source)
        {
            Parser.Parser parser = new(source);
            var root = parser.ParseCompilationUnit();
            DiagnosticBag diagnostics = parser.Diagnostics;

            Source = source;
            Diagnostics = diagnostics;
            Root = root;
        }

        public SourceText Source { get; }
        public DiagnosticBag Diagnostics { get; }
        public CompilationUnitSyntax Root { get; }
        public Token EofToken { get; }

        public static SyntaxTree Parse(SourceText source) => new SyntaxTree(source);
        public static SyntaxTree Parse(string source) => Parse(SourceText.From(source));
        public static IEnumerable<Token> ParseTokens(string source)
        {
            if (!string.IsNullOrWhiteSpace(source))
            {
                Lexer.Lexer lexer = new(SourceText.From(source));
                while (true)
                {
                    Token token = lexer.Lex();
                    if (token.Kind == SyntaxKind.EOF) break;
                    yield return token;
                }
            }
        }
    }
}
