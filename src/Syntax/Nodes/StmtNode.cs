using Stonylang.Lexer;
using System.Collections.Immutable;

namespace Stonylang.Parser
{
    public abstract class StmtNode : Node { }

    public sealed class ExpressionStmt : StmtNode
    {
        public ExpressionStmt(ExprNode expression) => Expression = expression;
        public override SyntaxKind Kind => SyntaxKind.ExpressionStmt;
        public ExprNode Expression { get; }
    }

    public sealed class BlockStmt : StmtNode
    {
        public BlockStmt(Token lBrace, ImmutableArray<StmtNode> statements, Token rBrace)
        {
            LBrace = lBrace;
            Statements = statements;
            RBrace = rBrace;
        }

        public override SyntaxKind Kind => SyntaxKind.BlockStmt;
        public Token LBrace { get; }
        public ImmutableArray<StmtNode> Statements { get; }
        public Token RBrace { get; }
    }

    public sealed class VariableStmt : StmtNode
    {
        public VariableStmt(Token keyword, Token identifier, ExprNode initializer, bool isMut)
        {
            Keyword = keyword;
            Identifier = identifier;
            Initializer = initializer;
            IsMut = isMut;
        }

        public override SyntaxKind Kind => SyntaxKind.VariableStmt;
        public Token Keyword { get; }
        public Token Identifier { get; }
        public ExprNode Initializer { get; }
        public bool IsMut { get; }
    }

    public sealed class IfStmt : StmtNode
    {
        public IfStmt(Token keyword, ExprNode condition, BlockStmt thenBranch, ElseClauseStmt elseBranch)
        {
            Keyword = keyword;
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        public override SyntaxKind Kind => SyntaxKind.IfStmt;
        public Token Keyword { get; }
        public ExprNode Condition { get; }
        public BlockStmt ThenBranch { get; }
        public ElseClauseStmt ElseBranch { get; }
    }

    public sealed class ElseClauseStmt : StmtNode
    {
        public ElseClauseStmt(Token elseKeyword, BlockStmt elseBranch)
        {
            ElseKeyword = elseKeyword;
            ElseBranch = elseBranch;
        }

        public override SyntaxKind Kind => SyntaxKind.ElseClauseStmt;
        public Token ElseKeyword { get; }
        public BlockStmt ElseBranch { get; }
    }

    public sealed class WhileStmt : StmtNode
    {
        public WhileStmt(Token whileKeyword, ExprNode condition, BlockStmt thenBranch, bool isDoWhile)
        {
            WhileKeyword = whileKeyword;
            Condition = condition;
            ThenBranch = thenBranch;
            IsDoWhile = isDoWhile;
        }

        public override SyntaxKind Kind => SyntaxKind.WhileStmt;
        public Token WhileKeyword { get; }
        public ExprNode Condition { get; }
        public BlockStmt ThenBranch { get; }
        public bool IsDoWhile { get; }
    }

    public sealed class ForStmt : StmtNode
    {
        public ForStmt(Token forKeyword, Token identifier, bool isMut, ExprNode initialValue, ExprNode range, BlockStmt stmt)
        {
            ForKeyword = forKeyword;
            Identifier = identifier;
            IsMut = isMut;
            InitialValue = initialValue;
            Range = range;
            Stmt = stmt;
        }

        public override SyntaxKind Kind => SyntaxKind.ForStmt;
        public Token ForKeyword { get; }
        public Token Identifier { get; }
        public bool IsMut { get; }
        public ExprNode InitialValue { get; }
        public ExprNode Range { get; }
        public BlockStmt Stmt { get; }
    }
}
