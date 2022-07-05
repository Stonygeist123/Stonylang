using Stonylang_CSharp.Utility;
using System.Collections.Generic;

namespace Stonylang_CSharp.Lexer
{
    internal sealed class Lexer
    {
        private readonly string _source;
        private readonly int _line = 1;
        private int _position = 0;
        private readonly DiagnosticBag _diagnostics = new();
        public DiagnosticBag Diagnostics => _diagnostics;
        private char Current => Peek();
        private char Peek(int offset = 0) => _position + offset >= _source.Length ? '\0' : _source[_position + offset];

        public Lexer(string source) => _source = source;

        private Token Number()
        {
            int start = _position;
            while (char.IsDigit(Current)) Advance();
            if (!int.TryParse(_source[start.._position], out int v))
                _diagnostics.Report(_source, new(start, _position - start), _line, $"Invalid integer literal \"{ _source[start.._position]}\".", "SyntaxException", LogLevel.Error);
            return new Token(SyntaxKind.Number, _source[start.._position], v, new(start, _position - start), _line);
        }

        public Token Lex()
        {
            if (_position >= _source.Length) return new Token(SyntaxKind.EOF, "\0", null, new(_position, 0), _line);
            if (char.IsDigit(Current)) return Number();
            if (char.IsWhiteSpace(Current))
            {
                int start = _position;
                while (char.IsWhiteSpace(Current)) Advance();
                return new Token(SyntaxKind.Whitespace, _source[start.._position], null, new(start, _position - start), _line);
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
                    return new Token(SyntaxKind.Plus, "+", null, new(_position++, 1), _line);
                case '-':
                    return new Token(SyntaxKind.Minus, "-", null, new(_position++, 1), _line);
                case '*':
                    if (Peek(1) == '*')
                    {
                        if (Peek(1) == '=')
                            return new Token(SyntaxKind.PowerEq, "**=", null, new(_position += 2, 2), _line);
                        return new Token(SyntaxKind.Power, "**", null, new(_position += 2, 2), _line);
                    }
                    if (Peek(1) == '=')
                        return new Token(SyntaxKind.StarEq, "*=", null, new(_position += 2, 2), _line);
                    return new Token(SyntaxKind.Star, "*", null, new(_position++, 1), _line);
                case '/':
                    return new Token(SyntaxKind.Slash, "/", null, new(_position++, 1), _line);
                case '(':
                    return new Token(SyntaxKind.LParen, "(", null, new(_position++, 1), _line);
                case ')':
                    return new Token(SyntaxKind.RParen, ")", null, new(_position++, 1), _line);
                case '&':
                    if (Peek(1) == '&')
                        return new Token(SyntaxKind.LogicalAnd, "&&", null, new(_position += 2, 2), _line);
                    else if (Peek(1) == '=')
                        return new Token(SyntaxKind.AndEq, "&=", null, new(_position += 2, 2), _line);
                    return new Token(SyntaxKind.And, "&", null, new(_position++, 1), _line);
                case '|':
                    if (Peek(1) == '|')
                        return new Token(SyntaxKind.LogicalOr, "||", null, new(_position += 2, 2), _line);
                    else if (Peek(1) == '=')
                        return new Token(SyntaxKind.OrEq, "|=", null, new(_position += 2, 2), _line);
                    return new Token(SyntaxKind.Or, "|", null, new(_position++, 1), _line);
                case '=':
                    if (Peek(1) == '=')
                        return new Token(SyntaxKind.EqEq, "==", null, new(_position += 2, 2), _line);
                    return new Token(SyntaxKind.Equals, "=", null, new(_position++, 1), _line);
                case '!':
                    if (Peek(1) == '=')
                        return new Token(SyntaxKind.NotEq, "!=", null, new(_position += 2, 2), _line);
                    return new Token(SyntaxKind.Not, "~", null, new(_position++, 1), _line);
                case '^':
                    if (Peek(1) == '=')
                        return new Token(SyntaxKind.XorEq, "^=", null, new(_position += 2, 2), _line);
                    return new Token(SyntaxKind.Xor, "^", null, new(_position++, 1), _line);
                case '~':
                    return new Token(SyntaxKind.Inv, "~", null, new(_position++, 1), _line);
                default:
                    _diagnostics.Report(_source, new(_position, 1), _line, $"Unexpected character '{Current}'.", "SyntaxException", LogLevel.Error);
                    return new Token(SyntaxKind.Bad, "", null, new(_position++, 1), _line);
            };
        }

        private int Advance() => _position++;
    }
}
