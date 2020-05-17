using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ColumnSpecParsingExample
{
    public static partial class ColumnAnnotationParser
    {
        public static Result<ImmutableArray<ImmutableArray<ColumnSort>>> ParseGroupingLevels(string annotationValue)
        {
            var groupingLevels = ImmutableArray.CreateBuilder<ImmutableArray<ColumnSort>>();
            var currentGroupingLevel = ImmutableArray.CreateBuilder<ColumnSort>();

            var context = new LexingContext(new Lexer(annotationValue));

            if (!context.TryLex(SyntaxKind.OpeningParenthesisToken, SyntaxKind.EndOfInput))
                return Result.Error(context.ErrorMessage!);

            var usedFieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (context.CurrentToken.Kind != SyntaxKind.EndOfInput)
            {
                if (!context.TryLex(SyntaxKind.Identifier))
                    return Result.Error(context.ErrorMessage!);

                var fieldName = context.CurrentToken.Value!;
                var directionKeyword = (SyntaxKind?)null;

                if (!context.TryLex(SyntaxKind.AscKeyword, SyntaxKind.DescKeyword, SyntaxKind.CommaToken, SyntaxKind.ClosingParenthesisToken))
                    return Result.Error(context.ErrorMessage!);

                switch (context.CurrentToken.Kind)
                {
                    case SyntaxKind.AscKeyword:
                    case SyntaxKind.DescKeyword:
                        directionKeyword = context.CurrentToken.Kind;

                        if (!context.TryLex(SyntaxKind.CommaToken, SyntaxKind.ClosingParenthesisToken))
                            return Result.Error(context.ErrorMessage!);
                        break;
                }

                if (!usedFieldNames.Add(fieldName))
                    return Result.Error($"The field name ‘{fieldName}’ appears more than once.");

                currentGroupingLevel.Add(new ColumnSort(
                    fieldName,
                    ascendingOrder: (directionKeyword ?? SyntaxKind.AscKeyword) == SyntaxKind.AscKeyword));

                if (context.CurrentToken.Kind == SyntaxKind.ClosingParenthesisToken)
                {
                    groupingLevels.Add(currentGroupingLevel.ToImmutable());
                    currentGroupingLevel.Clear();

                    if (!context.TryLex(SyntaxKind.CommaToken, SyntaxKind.EndOfInput))
                        return Result.Error(context.ErrorMessage!);

                    if (context.CurrentToken.Kind == SyntaxKind.CommaToken)
                    {
                        if (!context.TryLex(SyntaxKind.OpeningParenthesisToken))
                            return Result.Error(context.ErrorMessage!);
                    }
                }
            }

            return Result.Success(groupingLevels.ToImmutable());
        }

        public static Result<ImmutableArray<ColumnSort>> ParseSortLevel(string annotationValue)
        {
            var sortLevel = ImmutableArray.CreateBuilder<ColumnSort>();

            var context = new LexingContext(new Lexer(annotationValue));

            var usedFieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!context.TryLex(SyntaxKind.Identifier, SyntaxKind.EndOfInput))
                return Result.Error(context.ErrorMessage!);

            while (context.CurrentToken.Kind != SyntaxKind.EndOfInput)
            {
                var fieldName = context.CurrentToken.Value!;
                var directionKeyword = (SyntaxKind?)null;

                if (!context.TryLex(SyntaxKind.AscKeyword, SyntaxKind.DescKeyword, SyntaxKind.CommaToken, SyntaxKind.EndOfInput))
                    return Result.Error(context.ErrorMessage!);

                switch (context.CurrentToken.Kind)
                {
                    case SyntaxKind.AscKeyword:
                    case SyntaxKind.DescKeyword:
                        directionKeyword = context.CurrentToken.Kind;

                        if (!context.TryLex(SyntaxKind.CommaToken, SyntaxKind.EndOfInput))
                            return Result.Error(context.ErrorMessage!);
                        break;
                }

                if (!usedFieldNames.Add(fieldName))
                    return Result.Error($"The field name ‘{fieldName}’ appears more than once.");

                sortLevel.Add(new ColumnSort(
                    fieldName,
                    ascendingOrder: (directionKeyword ?? SyntaxKind.AscKeyword) == SyntaxKind.AscKeyword));

                if (context.CurrentToken.Kind == SyntaxKind.CommaToken)
                {
                    if (!context.TryLex(SyntaxKind.Identifier))
                        return Result.Error(context.ErrorMessage!);
                }
            }

            return Result.Success(sortLevel.ToImmutable());
        }
    }
}
