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
        public static ImmutableArray<Token> ParseTokens(string source) => ParseTokens(source, out _);
        public static ImmutableArray<Token> ParseTokens(string source, out DiagnosticBag diagnostics)
        {
            if (!string.IsNullOrWhiteSpace(source))
            {
                static IEnumerable<Token> LexTokens(Lexer.Lexer lexer)
                {
                    while (true)
                    {
                        Token token = lexer.Lex();
                        if (token.Kind == SyntaxKind.EOF) break;
                        yield return token;
                    }
                }

                Lexer.Lexer lexer = new(SourceText.From(source));
                ImmutableArray<Token> result = LexTokens(lexer).ToImmutableArray();
                diagnostics = lexer.Diagnostics;
                return result;
            }

            diagnostics = null;
            return ImmutableArray<Token>.Empty;
        }
    }
}
