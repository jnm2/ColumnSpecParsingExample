namespace ColumnSpecParsingExample
{
    partial class ColumnAnnotationParser
    {
        private enum SyntaxKind
        {
            /// <summary>
            /// The end of the string is represented as a token for simplicity.
            /// </summary>
            EndOfInput,
            /// <summary>
            /// This specifically represents a lexing error, not a parsing error.
            /// The error message is stored in <see cref="SyntaxToken.Value"/>.
            /// </summary>
            Error,
            OpeningParenthesisToken,
            ClosingParenthesisToken,
            CommaToken,
            AscKeyword,
            DescKeyword,
            /// <summary>
            /// The identifier name is stored in <see cref="SyntaxToken.Value"/>.
            /// </summary>
            Identifier,
        }
    }
}
