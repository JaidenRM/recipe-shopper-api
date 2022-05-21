using RecipeShopper.Application.Extensions;
using RecipeShopper.Domain.Enums;
using Shouldly;
using System;
using System.Text;
using Xunit;

namespace RecipeShopper.UnitTests.Extensions.Enums
{
    public class MeasurementUnitEnumTests
    {
        [Fact]
        public void Should_convert_all_enums_to_strings()
        {
            foreach(MeasurementUnit unitEnum in Enum.GetValues(typeof(MeasurementUnit)))
            {
                unitEnum.ToFriendlyString().ShouldNotBeNull();
            }
        }

        [Fact]
        public void Should_convert_all_enum_strings_back_to_enums()
        {
            foreach (MeasurementUnit unitEnum in Enum.GetValues(typeof(MeasurementUnit)))
            {
                var enumStr = unitEnum.ToFriendlyString();
                Enum.IsDefined(enumStr.ToEnum<MeasurementUnit>()).ShouldBe(true);
            }
        }

        [Fact]
        public void Should_convert_all_enum_strings_back_to_enums_insensitive_to_case()
        {
            foreach (MeasurementUnit unitEnum in Enum.GetValues(typeof(MeasurementUnit)))
            {
                var enumStr = JumbleCase(unitEnum.ToFriendlyString());
                Enum.IsDefined(enumStr.ToEnum<MeasurementUnit>()).ShouldBe(true);
            }
        }

        private string JumbleCase(string word)
        {
            var strBuilder = new StringBuilder();

            for(int i = 0; i < word.Length; i++)
            {
                if (i % 2 == 0) strBuilder.Append(word[i].ToString().ToUpper());
                else strBuilder.Append(word[i].ToString().ToLower());
            }

            return strBuilder.ToString();
        }
    }
}
