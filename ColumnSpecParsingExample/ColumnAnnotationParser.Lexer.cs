using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ColumnSpecParsingExample
{
    partial class ColumnAnnotationParser
    {
        /// <summary>
        /// Tokenizes a string so that the parsing logic is simpler, dealing with tokens rather than characters.
        /// </summary>
        private sealed class Lexer
        {
            private static readonly ImmutableDictionary<string, SyntaxKind> Keywords = ImmutableDictionary.CreateRange(
                StringComparer.OrdinalIgnoreCase,
                new[]
                {
                    KeyValuePair.Create("asc", SyntaxKind.AscKeyword),
                    KeyValuePair.Create("desc", SyntaxKind.DescKeyword),
                });

            private readonly string text;
            private int position;

            public Lexer(string text)
            {
                this.text = text ?? string.Empty;
            }

            public SyntaxToken Lex()
            {
                while (true)
                {
                    if (position >= text.Length) return new SyntaxToken(SyntaxKind.EndOfInput);

                    var c = text[position];
                    position++;

                    switch (c)
                    {
                        case '(':
                            return new SyntaxToken(SyntaxKind.OpeningParenthesisToken);
                        case ')':
                            return new SyntaxToken(SyntaxKind.ClosingParenthesisToken);
                        case ',':
                            return new SyntaxToken(SyntaxKind.CommaToken);
                        default:
                            if (char.IsWhiteSpace(c)) break;

                            if (IsValidIdentifierStartingCharacter(c))
                            {
                                var startPosition = position - 1;

                                while (position < text.Length && IsValidIdentifierCharacter(text[position]))
                                    position++;

                                var keywordOrIdentifierValue = text[startPosition..position];

                                return new SyntaxToken(
                                    Keywords.GetValueOrDefault(keywordOrIdentifierValue, defaultValue: SyntaxKind.Identifier),
                                    keywordOrIdentifierValue);
                            }

                            return new SyntaxToken(SyntaxKind.Error, $"Unrecognized character ‘{c}’.");
                    }
                }
            }

            private static bool IsValidIdentifierStartingCharacter(char c)
            {
                return char.IsLetter(c) || c == '_';
            }

            private static bool IsValidIdentifierCharacter(char c)
            {
                return char.IsLetterOrDigit(c) || c == '_';
            }
        }
    }
}
