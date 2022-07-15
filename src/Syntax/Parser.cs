using Stonylang.Lexer;
using Stonylang.SyntaxFacts;
using Stonylang.Utility;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Stonylang.Parser
{
    internal sealed class Parser
    {
        private int _position = 0;
        private readonly SourceText _source;
        private readonly ImmutableArray<Token> _tokens;
        private readonly DiagnosticBag _diagnostics = new();
        public DiagnosticBag Diagnostics => _diagnostics;

        public Parser(SourceText source)
        {
            List<Token> tokens = new();
            Lexer.Lexer lexer = new(source);
            Token token;
            do
            {
                token = lexer.Lex();
                if (token.Kind != SyntaxKind.Whitespace && token.Kind != SyntaxKind.Bad) tokens.Add(token);
            } while (token.Kind != SyntaxKind.EOF);

            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagnostics);
            _source = source;
        }

        public CompilationUnitSyntax ParseCompilationUnit() => new(ParseStatement(), Match(SyntaxKind.EOF));

        public StmtNode ParseStatement()
        {
            return Current.Kind switch
            {
                SyntaxKind.LBrace => ParseBlockStmt(),
                SyntaxKind.VarKeyword => ParseVariableStmt(),
                SyntaxKind.IfKeyword => ParseIfStmt(),
                SyntaxKind.DoKeyword or SyntaxKind.WhileKeyword => ParseWhileStmt(),
                SyntaxKind.ForKeyword => ParseForStmt(),
                _ => ParseExpressionStmt()
            };
        }

        private BlockStmt ParseBlockStmt()
        {
            ImmutableArray<StmtNode>.Builder statements = ImmutableArray.CreateBuilder<StmtNode>();
            Token lBrace = Match(SyntaxKind.LBrace);
            while (Current.Kind != SyntaxKind.EOF && Current.Kind != SyntaxKind.RBrace)
            {
                Token startToken = Current;
                statements.Add(ParseStatement());

                if (Current == startToken)
                    Advance();
            }
            return new BlockStmt(lBrace, statements.ToImmutable(), Match(SyntaxKind.RBrace));
        }

        private VariableStmt ParseVariableStmt()
        {
            Token varKeyword = Advance();
            Token mutKeyword = Current.Kind == SyntaxKind.MutKeyword ? Match(SyntaxKind.MutKeyword) : null;
            Token identifier = Match(SyntaxKind.Identifier);
            Token type = null;
            if (Current.Kind == SyntaxKind.Colon)
            {
                Match(SyntaxKind.Colon);
                type = Match(SyntaxKind.Identifier);
            }
            Match(SyntaxKind.Equals);
            ExprNode initializr = ParseExpression();
            return new VariableStmt(varKeyword, identifier, initializr, mutKeyword != null, type);
        }

        private IfStmt ParseIfStmt()
        {
            Token keyword = Match(SyntaxKind.IfKeyword);
            ExprNode condition = ParseExpression();
            BlockStmt thenBranch = ParseBlockStmt();
            ElseClauseStmt elseBranch = Current.Kind == SyntaxKind.ElseKeyword ? new(Match(SyntaxKind.ElseKeyword), ParseBlockStmt()) : null;
            return new(keyword, condition, thenBranch, elseBranch);
        }

        private WhileStmt ParseWhileStmt()
        {
            if (Current.Kind == SyntaxKind.DoKeyword)
            {
                Token doKeyword = Match(SyntaxKind.DoKeyword);
                BlockStmt thenBranch = ParseBlockStmt();
                Token keyword = Match(SyntaxKind.WhileKeyword);
                return new(keyword, ParseExpression(), thenBranch, doKeyword);
            }
            return new(Match(SyntaxKind.WhileKeyword), Current.Kind == SyntaxKind.LBrace ? null : ParseExpression(), ParseBlockStmt());
        }

        private ForStmt ParseForStmt()
        {
            Token keyword = Match(SyntaxKind.ForKeyword);
            bool isMut = Current.Kind == SyntaxKind.MutKeyword;
            if (isMut)
                Match(SyntaxKind.MutKeyword);

            Token identifier = Match(SyntaxKind.Identifier);
            Match(SyntaxKind.Equals);
            ExprNode initialValue = ParseExpression();
            Match(SyntaxKind.ToKeyword);
            ExprNode range = ParseExpression();
            BlockStmt stmt = ParseBlockStmt();
            return new(keyword, identifier, isMut, initialValue, range, stmt);
        }

        private ExpressionStmt ParseExpressionStmt() => new(ParseExpression());

        private ExprNode ParseExpression() => ParseAssignmentExpr();
        private ExprNode ParseAssignmentExpr()
        {
            if (Current.Kind == SyntaxKind.Identifier)
            {
                if (Peek(1).Kind == SyntaxKind.Equals)
                {
                    Token name = Advance();
                    Token op = Advance();
                    ExprNode right = ParseAssignmentExpr();
                    return new AssignmentExpr(name, op, right);
                }
                else if (IsAssignment(Peek(1).Kind))
                {
                    Token name = Advance();
                    if (name.Kind != SyntaxKind.Identifier)
                        _diagnostics.Report(_source, Peek(1).Span,
                            $"The left-hand side of an assignment must be a variable.", "AssignmentException", LogLevel.Error);
                    Token eqToken = new(SyntaxKind.Equals, "=", null, new(Current.Span.Start, 1), Current.Line);
                    Token binOp = new(Advance().Kind switch
                    {
                        SyntaxKind.PlusEq => SyntaxKind.Plus,
                        SyntaxKind.MinusEq => SyntaxKind.Minus,
                        SyntaxKind.StarEq => SyntaxKind.Star,
                        SyntaxKind.SlashEq => SyntaxKind.Slash,
                        SyntaxKind.PowerEq => SyntaxKind.Power,
                        SyntaxKind.ModEq => SyntaxKind.Mod,
                        SyntaxKind.RshEq => SyntaxKind.Rsh,
                        SyntaxKind.LshEq => SyntaxKind.Lsh,
                        SyntaxKind.AndEq => SyntaxKind.And,
                        SyntaxKind.OrEq => SyntaxKind.Or,
                        SyntaxKind.XorEq => SyntaxKind.Xor,
                        _ => SyntaxKind.Bad
                    }, eqToken.Lexeme[0..^1], null, new(eqToken.Span.Start, eqToken.Span.Length - 1), eqToken.Line);
                    ExprNode right = ParseAssignmentExpr();
                    return new AssignmentExpr(name, eqToken, new BinaryExpr(new NameExpr(name), binOp, right));
                }
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
                left = ParsePrimaryExpression();

            while (true)
            {
                int precende = Current.Kind.GetBinaryOpPrecedence();
                if (precende == 0 || precende <= parentPrecedence) break;
                left = new BinaryExpr(left, Advance(), ParseBinaryExpression(precende));
            }

            return left;
        }

        public ExprNode ParsePrimaryExpression() => Current.Kind switch
        {
            SyntaxKind.LParen => ParseGroupingExpr(),
            SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => ParseBoolLiteral(),
            SyntaxKind.Int or SyntaxKind.Float => ParseNumberExpr(),
            SyntaxKind.String => ParseStringLiteral(),
            SyntaxKind.Identifier or _ => ParseNameOrCallExpr(),
        };

        private ExprNode ParseGroupingExpr()
        {
            Advance();
            ExprNode expr = ParseExpression();
            Match(SyntaxKind.RParen);
            return expr;
        }

        private ExprNode ParseBoolLiteral()
        {
            bool isTrue = Current.Kind == SyntaxKind.TrueKeyword;
            Token kw = isTrue ? Match(SyntaxKind.TrueKeyword) : Match(SyntaxKind.FalseKeyword);
            return new LiteralExpr(kw, isTrue);
        }

        private ExprNode ParseStringLiteral() => new LiteralExpr(Match(SyntaxKind.String));
        private ExprNode ParseNumberExpr() => new LiteralExpr(Current.Kind == SyntaxKind.Int ? Match(SyntaxKind.Int) : Match(SyntaxKind.Float));
        private ExprNode ParseNameOrCallExpr()
        {
            if (Current.Kind == SyntaxKind.Identifier && Peek(1).Kind == SyntaxKind.LParen)
                return ParseCallExpr();
            return new NameExpr(Match(SyntaxKind.Identifier, "Expected expression."));
        }

        private ExprNode ParseCallExpr()
        {
            Token identifier = Match(SyntaxKind.Identifier);
            Token lParen = Match(SyntaxKind.LParen);
            SeparatedSyntaxList<ExprNode> arguments = ParseArguments();
            return new CallExpr(identifier, lParen, arguments, Match(SyntaxKind.RParen));
        }

        private SeparatedSyntaxList<ExprNode> ParseArguments()
        {
            ImmutableArray<Node>.Builder nodesAndSeparators = ImmutableArray.CreateBuilder<Node>();
            while (Current.Kind != SyntaxKind.EOF && Current.Kind != SyntaxKind.RParen)
            {
                ExprNode expr = ParseExpression();
                nodesAndSeparators.Add(expr);

                if (Current.Kind != SyntaxKind.RParen)
                {
                    Token comma = Match(SyntaxKind.Comma);
                    nodesAndSeparators.Add(comma);
                }
            }
            return new(nodesAndSeparators.ToImmutableArray<Node>());
        }

        private Token Peek(int offset = 0) => _position + offset >= _tokens.Length ? _tokens.Last() : _tokens[_position + offset];
        private Token Current => Peek();
        private Token Advance()
        {
            Token c = Current;
            ++_position;
            return c;
        }

        private Token Match(SyntaxKind expected, string errorMessage = "")
        {
            if (Current.Kind == expected) return Advance();
            _diagnostics.Report(_source, Current.Span, errorMessage != "" ? errorMessage : $"Unexpected \"{Current.Kind.GetText() ?? Current.Kind.ToString()}\", expected \"{expected.GetText() ?? expected.ToString()}\".", "SyntaxException", LogLevel.Error);
            return new Token(expected, Current.Lexeme, null, Current.Span, Current.Line);
        }

        private static bool IsAssignment(SyntaxKind kind)
        {
            List<SyntaxKind> kinds = new()
            {
                SyntaxKind.PlusEq,
                SyntaxKind.MinusEq,
                SyntaxKind.StarEq,
                SyntaxKind.SlashEq,
                SyntaxKind.PowerEq,
                SyntaxKind.ModEq,
                SyntaxKind.RshEq,
                SyntaxKind.LshEq,
                SyntaxKind.AndEq,
                SyntaxKind.OrEq,
                SyntaxKind.XorEq
            };
            return kinds.Contains(kind);
        }
    }
}
