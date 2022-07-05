using System.Collections.Generic;
using System.Linq;
using Stonylang_CSharp.Utility;
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
                if (token.Kind != SyntaxKind.Whitespace && token.Kind != SyntaxKind.Bad) _tokens.Add(token);
            } while (token.Kind != SyntaxKind.EOF);

            _diagnostics.AddRange(lexer.Diagnostics);
            _source = source;
        }
        public SyntaxTree.SyntaxTree Parse()
        {
            ExprNode expr = ParseExpression();
            Token eofToken = Match(SyntaxKind.EOF);
            return new SyntaxTree.SyntaxTree(_diagnostics, expr, eofToken);
        }

        private ExprNode ParseExpression()
        {
            return ParseAssignmentExpr();
        }

        private ExprNode ParseAssignmentExpr()
        {
            if (Current.Kind == SyntaxKind.Identifier && Peek(1).Kind == SyntaxKind.Equals)
            {
                Token name = Advance();
                Token op = Advance();
                ExprNode right = ParseAssignmentExpr();
                return new AssignmentExpr(name, op, right);
            }

            return ParseBinaryExpression();
        }
        private ExprNode ParseBinaryExpression(int parentPrecedence = 0)
        {
            ExprNode left;
            int unOpPrecedence = Current.Kind.GetUnaryOpPrecedence();
            if (unOpPrecedence != 0 && unOpPrecedence >= parentPrecedence)
                left = new UnaryExpr(Advance(), ParseBinaryExpression(unOpPrecedence));
            else
                left = ParsePrimary();

            while (true)
            {
                int precende = Current.Kind.GetBinaryOpPrecedence();
                if (precende == 0 || precende <= parentPrecedence) break;
                left = new BinaryExpr(left, Advance(), ParseBinaryExpression(precende));
            }

            return left;
        }

        public ExprNode ParsePrimary()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.LParen:
                    {
                        Advance();
                        ExprNode expr = ParseExpression();
                        Match(SyntaxKind.RParen);
                        return expr;
                    }
                case SyntaxKind.Identifier:
                    return new NameExpr(Advance());
                case SyntaxKind.True:
                case SyntaxKind.False:
                    return new LiteralExpr(Advance(), Peek(-1).Kind == SyntaxKind.True);
                default:
                    return new LiteralExpr(Match(SyntaxKind.Number));
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
        private Token Match(SyntaxKind kind)
        {
            if (Current.Kind == kind) return Advance();
            _diagnostics.Report(_source, Current.Span, Current.Line, $"Unexpected \"{Current.Kind}\", expected \"{kind}\".", "SyntaxException", LogLevel.Error);
            return new Token(kind, Current.Lexeme, null, Current.Span, Current.Line);
        }
    }
}
