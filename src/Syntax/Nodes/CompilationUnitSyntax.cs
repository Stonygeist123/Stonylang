using Stonylang.Lexer;

namespace Stonylang.Parser
{
    public sealed class CompilationUnitSyntax : Node
    {
        public CompilationUnitSyntax(StmtNode statement, Token eofToken)
        {
            Statement = statement;
            EofToken = eofToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public StmtNode Statement { get; }
        public Token EofToken { get; }
    }

}
