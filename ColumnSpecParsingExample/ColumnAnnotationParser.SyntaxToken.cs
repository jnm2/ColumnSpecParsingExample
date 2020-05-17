namespace ColumnSpecParsingExample
{
    partial class ColumnAnnotationParser
    {
        private readonly struct SyntaxToken
        {
            public SyntaxToken(SyntaxKind kind, string? value = null)
            {
                Kind = kind;
                Value = value;
            }

            public SyntaxKind Kind { get; }

            /// <summary>
            /// An arbitrary value whose meaning is determined by <see cref="Kind"/>.
            /// </summary>
            public string? Value { get; }
        }
    }
}
