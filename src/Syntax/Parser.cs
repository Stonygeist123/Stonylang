using System.Collections.Generic;
using System.Linq;
using Stonylang_CSharp.Diagnostics;
using Stonylang_CSharp.Lexer;
using Stonylang_CSharp.SyntaxFacts;

namespace Stonylang_CSharp.Parser
{
    internal sealed class Parser
    {
        private readonly List<Token> _tokens = new();
        private int _position = 0;
        private readonly DiagnosticBag _diagnostics = new();
        private readonly string _source;

        public DiagnosticBag Diagnostics => _diagnostics;
        public Parser(string source)
        {
            Lexer.Lexer lexer = new(source);
            Token token;
            do
            {
                token = lexer.Lex();
                if (token.Kind != TokenKind.Whitespace && token.Kind != TokenKind.Bad) _tokens.Add(token);
            } while (token.Kind != TokenKind.EOF);

            _diagnostics.AddRange(lexer.Diagnostics);
            _source = source;
        }
        public SyntaxTree.SyntaxTree Parse()
        {
            ExprNode expr = ParseExpression();
            Token eofToken = Match(TokenKind.EOF);
            return new SyntaxTree.SyntaxTree(_diagnostics, expr, eofToken);
        }

        private ExprNode ParseExpression(int parentPrecedence = 0)
        {
            ExprNode left;
            int unOpPrecedence = Current.Kind.GetUnaryOpPrecedence();
            if (unOpPrecedence != 0 && unOpPrecedence >= parentPrecedence)
                left = new UnaryExpr(Advance(), ParseExpression(unOpPrecedence));
            else
                left = ParsePrimary();

            while (true)
            {
                int precende = Current.Kind.GetBinaryOpPrecedence();
                if (precende == 0 || precende <= parentPrecedence) break;
                left = new BinaryExpr(left, Advance(), ParseExpression(precende));
            }

            return left;
        }

        public ExprNode ParsePrimary()
        {
            switch (Current.Kind)
            {
                case TokenKind.LParen:
                    Advance();
                    ExprNode expr = ParseExpression();
                    Match(TokenKind.RParen);
                    return expr;
                case TokenKind.True:
                case TokenKind.False:
                    return new LiteralExpr(Advance(), Peek(-1).Kind == TokenKind.True);
                default: return new LiteralExpr(Match(TokenKind.Number));
            }
        }

        private Token Peek(int offset = 0) => _position + offset >= _tokens.Count ? _tokens.Last() : _tokens[_position + offset];
        private Token Current => Peek();
        private Token Advance()
        {
            Token c = Current;
            ++_position;
            return c;
        }
        private Token Match(TokenKind kind)
        {
            if (Current.Kind == kind) return Advance();
            _diagnostics.Report(_source, Current.Span, Current.Line, $"Unexpected \"{Current.Kind}\", expected \"{kind}\".", "SyntaxException", LogLevel.Error);
            return new Token(kind, Current.Lexeme, null, Current.Span, Current.Line);
        }
    }
}
