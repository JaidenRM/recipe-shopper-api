using RecipeShopper.Domain.Helpers;
using Shouldly;
using Xunit;

namespace RecipeShopper.UnitTests.Domain
{
    public class ResultTests
    {
        [Fact]
        public void Result_is_successful_with_no_data_or_message()
        {
            var result = ResultHelper.Success();

            result.Message.ShouldBeNull();
            result.IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Result_is_successful_with_message_and_no_data()
        {
            var message = "Failed! XD";
            var result = ResultHelper.Success(message);

            result.Message.ShouldBe(message);
            result.IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Result_is_successful_with_data_and_no_message()
        {
            var data = 42;
            var result = ResultHelper.SuccessWithData(data);

            result.Data.ShouldBe(data);
            result.Message.ShouldBeNull();
            result.IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Result_is_successful_with_data_and_message()
        {
            var data = 21;
            var message = "Hello, world!";
            var result = ResultHelper.SuccessWithData(data, message);

            result.Data.ShouldBe(data);
            result.Message.ShouldBe(message);
            result.IsSuccessful.ShouldBeTrue();
        }

        [Fact]
        public void Result_has_failed_with_no_data_or_message()
        {
            var result = ResultHelper.Fail();

            result.Message.ShouldBeNull();
            result.IsSuccessful.ShouldBeFalse();
        }

        [Fact]
        public void Result_has_failed_with_message_and_no_data()
        {
            var message = "Failed! XD";
            var result = ResultHelper.Fail(message);

            result.Message.ShouldBe(message);
            result.IsSuccessful.ShouldBeFalse();
        }

        [Fact]
        public void Result_has_failed_with_data_and_no_message()
        {
            var data = 42;
            var result = ResultHelper.FailWithData(data);

            result.Data.ShouldBe(data);
            result.Message.ShouldBeNull();
            result.IsSuccessful.ShouldBeFalse();
        }

        [Fact]
        public void Result_has_failed_with_data_and_message()
        {
            var data = 21;
            var message = "Hello, world!";
            var result = ResultHelper.FailWithData(data, message);

            result.Data.ShouldBe(data);
            result.Message.ShouldBe(message);
            result.IsSuccessful.ShouldBeFalse();
        }

        [Fact]
        public void Result_keeps_same_complex_object()
        {
            var partnerObj = new { test = "hi", digits = new[]{ 1, 4, 6 } };
            var complexObj = new { foo = "bar", age = 56, fruits = new[] { "apple", "pear", "grape" }, partner = partnerObj };

            var result = ResultHelper.SuccessWithData(complexObj);

            // Just to make sure we aren't comparing references
            var partnerObj2 = new { test = "hi", digits = new[] { 1, 4, 6 } };
            var complexObj2 = new { foo = "bar", age = 56, fruits = new[] { "apple", "pear", "grape" }, partner = partnerObj2 };

            result.Data.ShouldBeEquivalentTo(complexObj2);
        }
    }
}
