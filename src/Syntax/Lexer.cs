using Stonylang_CSharp.SyntaxFacts;
using Stonylang_CSharp.Utility;
using System.Collections.Generic;

namespace Stonylang_CSharp.Lexer
{
    internal sealed class Lexer
    {
        private readonly SourceText _source;
        private readonly DiagnosticBag _diagnostics = new();
        public DiagnosticBag Diagnostics => _diagnostics;

        private int _position = 0, _start = 0, _line = 1;
        private SyntaxKind _kind;
        private object _value;

        public Lexer(SourceText source) => _source = source;
        private char Current => Peek();
        private char Peek(int offset = 0) => _position + offset >= _source.Length ? '\0' : _source[_position + offset];

        private void ReadNumber()
        {
            while (char.IsDigit(Current)) Advance();
            if (!int.TryParse(_source.ToString()[_start.._position], out int value))
                _diagnostics.Report(_source, new(_start, _position - _start),
                    $"Invalid integer literal \"{ _source.ToString()[_start.._position]}\".", "SyntaxException", LogLevel.Error);

            _kind = SyntaxKind.Number;
            _value = value;
        }

        private void ReadWhitespace()
        {
            while (char.IsWhiteSpace(Current)) Advance();
            _kind = SyntaxKind.Whitespace;
        }

        public Token Lex()
        {
            _start = _position;
            _kind = SyntaxKind.Bad;
            _value = null;

            switch (Current)
            {
                case '\0':
                    _kind = SyntaxKind.EOF;
                    break;
                case '+':
                    if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.PlusEq;
                    }
                    else if (Peek(1) == '+')
                    {
                        _position += 2;
                        _kind = SyntaxKind.Increment;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.Plus;
                    }
                    break;
                case '-':
                    if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.MinusEq;
                    }
                    else if (Peek(1) == '-')
                    {
                        _position += 2;
                        _kind = SyntaxKind.Decrement;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.Minus;
                    }
                    break;
                case '*':
                    if (Peek(1) == '*')
                    {
                        if (Peek(2) == '=')
                        {
                            _position += 3;
                            _kind = SyntaxKind.PowerEq;
                        }
                        else
                        {
                            _position += 2;
                            _kind = SyntaxKind.Power;
                        }
                    }
                    else if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.StarEq;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.Star;
                    }
                    break;
                case '/':
                    if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.SlashEq;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.Slash;
                    }
                    break;
                case '%':
                    if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.ModEq;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.Mod;
                    }
                    break;
                case '(':
                    ++_position;
                    _kind = SyntaxKind.LParen;
                    break;
                case ')':
                    ++_position;
                    _kind = SyntaxKind.RParen;
                    break;
                case '&':
                    if (Peek(1) == '&')
                    {
                        _position += 2;
                        _kind = SyntaxKind.LogicalAnd;
                    }
                    else if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.AndEq;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.And;
                    }
                    break;
                case '|':
                    if (Peek(1) == '|')
                    {
                        _position += 2;
                        _kind = SyntaxKind.LogicalOr;
                    }
                    else if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.OrEq;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.Or;
                    }
                    break;
                case '^':
                    if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.XorEq;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.Xor;
                    }
                    break;
                case '~':
                    ++_position;
                    _kind = SyntaxKind.Inv;
                    break;
                case '=':
                    if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.EqEq;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.Equals;
                    }
                    break;
                case '!':
                    if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.NotEq;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.Not;
                    }
                    break;
                case '>':
                    if (Peek(1) == '>')
                    {
                        if (Peek(2) == '=')
                        {
                            _position += 3;
                            _kind = SyntaxKind.RshEq;
                        }
                        else
                        {
                            _position += 2;
                            _kind = SyntaxKind.Rsh;
                        }
                    }
                    else if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.GreaterEq;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.Greater;
                    }
                    break;
                case '<':
                    if (Peek(1) == '<')
                    {
                        if (Peek(2) == '=')
                        {
                            _position += 3;
                            _kind = SyntaxKind.LshEq;
                        }
                        else
                        {
                            _position += 2;
                            _kind = SyntaxKind.Lsh;
                        }
                    }
                    else if (Peek(1) == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.LessEq;
                    }
                    else
                    {
                        ++_position;
                        _kind = SyntaxKind.Less;
                    }
                    break;
                case ' ':
                case '\t':
                case '\r':
                    ReadWhitespace();
                    break;
                case '\n':
                    ReadWhitespace();
                    ++_line;
                    break;
                default:
                    if (char.IsDigit(Current)) ReadNumber();
                    else if (char.IsWhiteSpace(Current)) ReadWhitespace();
                    else if (char.IsLetter(Current))
                    {
                        while (char.IsLetter(Current)) Advance();
                        _kind = SyntaxFacts.SyntaxFacts.GetKeywordKind(_source.ToString()[_start.._position]);
                    }
                    else
                    {
                        _diagnostics.Report(_source, new(_start, 1), $"Unexpected character '{Current}'.", "SyntaxException", LogLevel.Error);
                        ++_position;
                    }
                    break;
            };
            return new Token(_kind, _kind.GetText() ?? _source.ToString()[_start.._position], _value, new(_start, _position - _start), _line);
        }

        private int Advance() => ++_position;
    }
}
