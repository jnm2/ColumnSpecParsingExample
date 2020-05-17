using System.ComponentModel;
using System.Linq;
using Humanizer;

namespace ColumnSpecParsingExample
{
    partial class ColumnAnnotationParser
    {
        /// <summary>
        /// Aids the lexing process by tracking the previous token in order to provide more explanatory error messages.
        /// </summary>
        private sealed class LexingContext
        {
            private readonly Lexer lexer;
            private SyntaxToken? previousToken;
            private SyntaxToken? currentToken;

            public LexingContext(Lexer lexer)
            {
                this.lexer = lexer;
            }

            public SyntaxToken CurrentToken => currentToken!.Value;

            public string? ErrorMessage { get; private set; }

            public bool TryLex(params SyntaxKind[] expectedKinds)
            {
                previousToken = currentToken;
                currentToken = lexer.Lex();

                if (expectedKinds.Contains(currentToken.Value.Kind))
                {
                    ErrorMessage = null;
                    return true;
                }

                ErrorMessage = CreateUnexpectedTokenMessage(previousToken, currentToken.Value, expectedKinds);
                return false;
            }

            private static string CreateUnexpectedTokenMessage(SyntaxToken? previousToken, SyntaxToken token, SyntaxKind[] expectedOptions)
            {
                if (token.Kind == SyntaxKind.Error) return token.Value!;

                var context = previousToken switch
                {
                    null => "at start of input",
                    { } notNull => "after " + DescribeSyntax(notNull),
                };

                return $"Expected {expectedOptions.Humanize(DescribeSyntax, "or")} {context}, but found {DescribeSyntax(token)}.";
            }

            private static string DescribeSyntax(SyntaxKind kind)
            {
                return kind switch
                {
                    SyntaxKind.EndOfInput => "end of input",
                    SyntaxKind.OpeningParenthesisToken => "‘(’",
                    SyntaxKind.ClosingParenthesisToken => "‘)’",
                    SyntaxKind.CommaToken => "comma",
                    SyntaxKind.AscKeyword => "keyword ‘asc’",
                    SyntaxKind.DescKeyword => "keyword ‘desc’",
                    SyntaxKind.Identifier => "identifier",
                    _ => throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(SyntaxKind)),
                };
            }

            private static string DescribeSyntax(SyntaxToken token)
            {
                return token.Kind == SyntaxKind.Identifier
                    ? $"identifier ‘{token.Value}’"
                    : DescribeSyntax(token.Kind);
            }
        }
    }
}
