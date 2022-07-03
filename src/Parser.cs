using System.Collections.Generic;
using System.Linq;
using Stonylang_CSharp.Lexer;

namespace Stonylang_CSharp.Parser
{
    class Parser
    {
        private readonly List<Token> _tokens = new();
        private int _position = 0;
        private readonly List<string> _diagnostics = new();
        public IEnumerable<string> Diagnostics => _diagnostics;
        public Parser(string source)
        {
            Lexer.Lexer lexer = new(source);
            Token token;
            do
            {
                token = lexer.GetToken();
                if (token.Kind != TokenKind.Whitespace && token.Kind != TokenKind.Bad) _tokens.Add(token);
            } while (token.Kind != TokenKind.EOF);

            _diagnostics.AddRange(lexer.Diagnostics);
        }
        public SyntaxTree.SyntaxTree Parse()
        {
            ExprNode expr = ParseTerm();
            Token eofToken = Match(TokenKind.EOF);
            return new SyntaxTree.SyntaxTree(_diagnostics, expr, eofToken);
        }

        public ExprNode ParseExpression() => ParseTerm();
        private ExprNode ParseTerm()
        {
            ExprNode left = ParseFactor();
            while (Current.Kind == TokenKind.Plus || Current.Kind == TokenKind.Minus)
            {
                Token op = Advance();
                ExprNode right = ParseFactor();
                left = new BinaryExpr(left, op, right);
            }

            return left;
        }

        private ExprNode ParseFactor()
        {
            ExprNode left = ParsePrimary();
            while (Current.Kind == TokenKind.Star || Current.Kind == TokenKind.Slash)
            {
                Token op = Advance();
                ExprNode right = ParsePrimary();
                left = new BinaryExpr(left, op, right);
            }

            return left;
        }

        public ExprNode ParsePrimary()
        {
            if (Current.Kind == TokenKind.LParen)
                return new GroupingExpr(Advance(), ParseExpression(), Match(TokenKind.RParen));
            Token number = Match(TokenKind.Number);
            return new NumberExpr(number);
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
            _diagnostics.Add($"Error: Unexpected token <{Current.Kind}>. Expected <{kind}>.");
            return new Token(kind, Current.Lexeme, null, Current.Position, Current.Line);
        }
    }
}
