using Stonylang_CSharp.Lexer;
using System.Collections.Immutable;

namespace Stonylang_CSharp.Parser
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
}
