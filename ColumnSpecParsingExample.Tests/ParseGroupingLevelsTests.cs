using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace ColumnSpecParsingExample.Tests
{
    public static class ParseGroupingLevelsTests
    {
        [Test]
        public static void Blank_string_produces_no_grouping_levels([Values(null, "", " \t\r\n ")] string text)
        {
            ColumnAnnotationParser.ParseGroupingLevels(text).Value.ShouldBeEmpty();
        }

        [Test]
        public static void Unrecognized_character_is_reported()
        {
            ColumnAnnotationParser.ParseGroupingLevels("([A], [B])").ShouldBe(
                Result.Error("Unrecognized character ‘[’."));
        }

        [Test]
        public static void There_can_be_multiple_levels_with_differing_numbers_of_columns()
        {
            var levels = ColumnAnnotationParser.ParseGroupingLevels("(B, A), (C), (E, F, D)").Value;
            levels.Length.ShouldBe(3);
            levels[0].Select(g => g.FieldName).ShouldBe(new[] { "B", "A" });
            levels[1].Select(g => g.FieldName).ShouldBe(new[] { "C" });
            levels[2].Select(g => g.FieldName).ShouldBe(new[] { "E", "F", "D" });
        }

        [Test]
        public static void Asc_and_desc_keywords_may_have_any_casing()
        {
            var levels = ColumnAnnotationParser.ParseGroupingLevels("(A ASC, B aSc), (C DESC, D dEsC)").Value;
            levels.Length.ShouldBe(2);
            levels[0][0].AscendingOrder.ShouldBeTrue();
            levels[0][1].AscendingOrder.ShouldBeTrue();
            levels[1][0].AscendingOrder.ShouldBeFalse();
            levels[1][1].AscendingOrder.ShouldBeFalse();
        }

        [Test]
        public static void Field_names_must_be_within_parentheses()
        {
            ColumnAnnotationParser.ParseGroupingLevels("A").ShouldBe(
                Result.Error("Expected ‘(’ or end of input at start of input, but found identifier ‘A’."));
        }

        [Test]
        public static void Field_names_must_be_separated_with_commas()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(A B)").ShouldBe(
                Result.Error("Expected keyword ‘asc’, keyword ‘desc’, comma, or ‘)’ after identifier ‘A’, but found identifier ‘B’."));
        }

        [Test]
        public static void Grouping_levels_must_be_separated_with_commas()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(A) (B)").ShouldBe(
                Result.Error("Expected comma or end of input after ‘)’, but found ‘(’."));
        }

        [Test]
        public static void Default_sort_is_ascending()
        {
            var levels = ColumnAnnotationParser.ParseGroupingLevels("(A)").Value;
            levels[0][0].AscendingOrder.ShouldBeTrue();
        }

        [Test]
        public static void Asc_and_desc_keywords_may_be_used()
        {
            var levels = ColumnAnnotationParser.ParseGroupingLevels("(A asc, B), (C, D desc)").Value;
            levels.Length.ShouldBe(2);
            levels[0][0].AscendingOrder.ShouldBeTrue();
            levels[1][1].AscendingOrder.ShouldBeFalse();
        }

        [Test]
        public static void Asc_keyword_may_not_be_used_twice_for_the_same_column()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(A asc asc)").ShouldBe(
                Result.Error("Expected comma or ‘)’ after keyword ‘asc’, but found keyword ‘asc’."));
        }

        [Test]
        public static void Desc_keyword_may_not_be_used_twice_for_the_same_column()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(A desc desc)").ShouldBe(
                Result.Error("Expected comma or ‘)’ after keyword ‘desc’, but found keyword ‘desc’."));
        }

        [Test]
        public static void Asc_and_desc_keywords_may_not_be_used_for_the_same_column()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(A asc desc)").ShouldBe(
                Result.Error("Expected comma or ‘)’ after keyword ‘asc’, but found keyword ‘desc’."));
        }

        [Test]
        public static void Asc_keyword_may_not_be_used_without_a_column_name()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(asc)").ShouldBe(
                Result.Error("Expected identifier after ‘(’, but found keyword ‘asc’."));
        }

        [Test]
        public static void Desc_keyword_may_not_be_used_without_a_column_name()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(desc)").ShouldBe(
                Result.Error("Expected identifier after ‘(’, but found keyword ‘desc’."));
        }

        [Test]
        public static void Asc_keyword_may_not_be_used_before_the_column_name()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(asc A)").ShouldBe(
                Result.Error("Expected identifier after ‘(’, but found keyword ‘asc’."));
        }

        [Test]
        public static void Desc_keyword_may_not_be_used_before_the_column_name()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(desc A)").ShouldBe(
                Result.Error("Expected identifier after ‘(’, but found keyword ‘desc’."));
        }

        [Test]
        public static void Asc_keyword_may_not_be_used_on_entire_level()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(A) asc").ShouldBe(
                Result.Error("Expected comma or end of input after ‘)’, but found keyword ‘asc’."));
        }

        [Test]
        public static void Desc_keyword_may_not_be_used_on_entire_level()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(A) desc").ShouldBe(
                Result.Error("Expected comma or end of input after ‘)’, but found keyword ‘desc’."));
        }

        [Test]
        public static void Empty_level_is_not_allowed()
        {
            ColumnAnnotationParser.ParseGroupingLevels("()").ShouldBe(
                Result.Error("Expected identifier after ‘(’, but found ‘)’."));
        }

        [Test]
        public static void Trailing_comma_is_not_allowed_within_parentheses()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(A, )").ShouldBe(
                Result.Error("Expected identifier after comma, but found ‘)’."));
        }

        [Test]
        public static void Trailing_comma_is_not_allowed_outside_parentheses()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(A),").ShouldBe(
                Result.Error("Expected ‘(’ after comma, but found end of input."));
        }

        [Test]
        public static void Field_names_may_contain_underscores([Values("_", "_A", "A_", "A_A")] string identifier)
        {
            var levels = ColumnAnnotationParser.ParseGroupingLevels("(" + identifier + ")").Value;
            levels.ShouldHaveSingleItem().ShouldHaveSingleItem().FieldName.ShouldBe(identifier);
        }

        [Test]
        public static void Field_names_may_contain_digits([Values("A0", "A0A")] string identifier)
        {
            var levels = ColumnAnnotationParser.ParseGroupingLevels("(" + identifier + ")").Value;
            levels.ShouldHaveSingleItem().ShouldHaveSingleItem().FieldName.ShouldBe(identifier);
        }

        [Test]
        public static void Field_names_may_not_start_with_digits([Values("0", "0A")] string identifier)
        {
            ColumnAnnotationParser.ParseGroupingLevels("(" + identifier + ")").ShouldBe(
                Result.Error("Unrecognized character ‘0’."));
        }

        [Test]
        public static void Field_names_must_not_appear_twice_in_the_same_level_regardless_of_casing()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(A, B, a)").ShouldBe(
                Result.Error("The field name ‘a’ appears more than once."));
        }

        [Test]
        public static void Field_names_must_not_appear_in_two_different_levels_regardless_of_casing()
        {
            ColumnAnnotationParser.ParseGroupingLevels("(a, B), (A, C)").ShouldBe(
                Result.Error("The field name ‘A’ appears more than once."));
        }
    }
}
