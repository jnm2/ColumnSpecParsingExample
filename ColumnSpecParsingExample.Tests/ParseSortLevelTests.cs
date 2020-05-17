using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace ColumnSpecParsingExample.Tests
{
    public static class ParseSortLevelTests
    {
        [Test]
        public static void Blank_string_produces_no_column_sorts([Values(null, "", " \t\r\n ")] string text)
        {
            ColumnAnnotationParser.ParseSortLevel(text).Value.ShouldBeEmpty();
        }

        [Test]
        public static void Unrecognized_character_is_reported()
        {
            ColumnAnnotationParser.ParseSortLevel("[A]").ShouldBe(
                Result.Error("Unrecognized character ‘[’."));
        }

        [Test]
        public static void There_can_be_multiple_column_sorts()
        {
            ColumnAnnotationParser.ParseSortLevel("B, A").Value.Select(l => l.FieldName)
                .ShouldBe(new[] { "B", "A" });
        }

        [Test]
        public static void Asc_and_desc_keywords_may_have_any_casing()
        {
            ColumnAnnotationParser.ParseSortLevel("A ASC, B aSc, C DESC, D dEsC").Value
                .Select(l => l.AscendingOrder).ShouldBe(new[] { true, true, false, false });
        }

        [Test]
        public static void Field_names_must_not_be_within_parentheses()
        {
            ColumnAnnotationParser.ParseSortLevel("(A)").ShouldBe(
                Result.Error("Expected identifier or end of input at start of input, but found ‘(’."));
        }

        [Test]
        public static void Field_names_must_be_separated_with_commas()
        {
            ColumnAnnotationParser.ParseSortLevel("A B").ShouldBe(
                Result.Error("Expected keyword ‘asc’, keyword ‘desc’, comma, or end of input after identifier ‘A’, but found identifier ‘B’."));
        }

        [Test]
        public static void Default_sort_is_ascending()
        {
            ColumnAnnotationParser.ParseSortLevel("A").Value[0].AscendingOrder.ShouldBeTrue();
        }

        [Test]
        public static void Asc_keyword_may_not_be_used_twice_for_the_same_column()
        {
            ColumnAnnotationParser.ParseSortLevel("A asc asc").ShouldBe(
                Result.Error("Expected comma or end of input after keyword ‘asc’, but found keyword ‘asc’."));
        }

        [Test]
        public static void Desc_keyword_may_not_be_used_twice_for_the_same_column()
        {
            ColumnAnnotationParser.ParseSortLevel("A desc desc").ShouldBe(
                Result.Error("Expected comma or end of input after keyword ‘desc’, but found keyword ‘desc’."));
        }

        [Test]
        public static void Asc_and_desc_keywords_may_not_be_used_for_the_same_column()
        {
            ColumnAnnotationParser.ParseSortLevel("A asc desc").ShouldBe(
                Result.Error("Expected comma or end of input after keyword ‘asc’, but found keyword ‘desc’."));
        }

        [Test]
        public static void Asc_keyword_may_not_be_used_without_a_column_name()
        {
            ColumnAnnotationParser.ParseSortLevel("asc").ShouldBe(
                Result.Error("Expected identifier or end of input at start of input, but found keyword ‘asc’."));
        }

        [Test]
        public static void Desc_keyword_may_not_be_used_without_a_column_name()
        {
            ColumnAnnotationParser.ParseSortLevel("desc").ShouldBe(
                Result.Error("Expected identifier or end of input at start of input, but found keyword ‘desc’."));
        }

        [Test]
        public static void Asc_keyword_may_not_be_used_before_the_column_name()
        {
            ColumnAnnotationParser.ParseSortLevel("asc A").ShouldBe(
                Result.Error("Expected identifier or end of input at start of input, but found keyword ‘asc’."));
        }

        [Test]
        public static void Desc_keyword_may_not_be_used_before_the_column_name()
        {
            ColumnAnnotationParser.ParseSortLevel("desc A").ShouldBe(
                Result.Error("Expected identifier or end of input at start of input, but found keyword ‘desc’."));
        }

        [Test]
        public static void Trailing_comma_is_not_allowed()
        {
            ColumnAnnotationParser.ParseSortLevel("A,").ShouldBe(
                Result.Error("Expected identifier after comma, but found end of input."));
        }

        [Test]
        public static void Field_names_may_contain_underscores([Values("_", "_A", "A_", "A_A")] string identifier)
        {
            ColumnAnnotationParser.ParseSortLevel(identifier).Value[0].FieldName.ShouldBe(identifier);
        }

        [Test]
        public static void Field_names_may_contain_digits([Values("A0", "A0A")] string identifier)
        {
            ColumnAnnotationParser.ParseSortLevel(identifier).Value[0].FieldName.ShouldBe(identifier);
        }

        [Test]
        public static void Field_names_may_not_start_with_digits([Values("0", "0A")] string identifier)
        {
            ColumnAnnotationParser.ParseSortLevel(identifier).ShouldBe(
                Result.Error("Unrecognized character ‘0’."));
        }

        [Test]
        public static void Field_names_must_not_appear_twice_regardless_of_casing()
        {
            ColumnAnnotationParser.ParseSortLevel("A, B, a").ShouldBe(
                Result.Error("The field name ‘a’ appears more than once."));
        }
    }
}
