using System.Collections.Generic;

namespace Stonylang_CSharp.Lexer
{
    internal sealed class Lexer
    {
        private readonly string _source;
        private readonly int _line = 1;
        private int _position = 0;
        private readonly List<string> _diagnostics = new();
        public IEnumerable<string> Diagnostics => _diagnostics;
        private char Current => _position >= _source.Length ? '\0' : _source[_position];

        public Lexer(string source) => _source = source;

        public Token Lex()
        {
            if (_position >= _source.Length) return new Token(TokenKind.EOF, "\0", null, new(_position, 0), _line);
            if (char.IsDigit(Current))
            {
                int start = _position;
                while (char.IsDigit(Current)) Advance();
                if (!int.TryParse(_source[start.._position], out int v))
                    _diagnostics.Add($"Invalid integer literal \"{ _source[start.._position]}\".");
                return new Token(TokenKind.Number, _source[start.._position], v, new(start, _position - start), _line);
            }
            if (char.IsWhiteSpace(Current))
            {
                int start = _position;
                while (char.IsWhiteSpace(Current)) Advance();
                return new Token(TokenKind.Whitespace, _source[start.._position], null, new(start, _position - start), _line);
            }
            if (char.IsLetter(Current))
            {
                int start = _position;
                while (char.IsLetter(Current)) Advance();
                return new Token(SyntaxFacts.SyntaxFacts.GetKeywordKind(_source[start.._position]), _source[start.._position], null, new(start, _position - start), _line);
            }

            switch (Current)
            {
                case '+':
                    return new Token(TokenKind.Plus, "+", null, new(_position++, 1), _line);
                case '-':
                    return new Token(TokenKind.Minus, "-", null, new(_position++, 1), _line);
                case '*':
                    return new Token(TokenKind.Star, "*", null, new(_position++, 1), _line);
                case '/':
                    return new Token(TokenKind.Slash, "/", null, new(_position++, 1), _line);
                case '(':
                    return new Token(TokenKind.LParen, "(", null, new(_position++, 1), _line);
                case ')':
                    return new Token(TokenKind.RParen, ")", null, new(_position++, 1), _line);
                case '~':
                    return new Token(TokenKind.Inv, "~", null, new(_position++, 1), _line);
                default:
                    _diagnostics.Add($"Error: Unexpected character '{Current}'.");
                    return new Token(TokenKind.Bad, "", null, new(_position++, 1), _line);
            };
        }

        private int Advance() => _position++;
    }
}
