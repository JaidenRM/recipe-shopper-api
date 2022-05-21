using FluentValidation;
using RecipeShopper.Features.Recipes;
using Shouldly;
using Xunit;

namespace RecipeShopper.UnitTests.Features.Recipes
{
    public class DeleteRecipeTests
    {
        private readonly AbstractValidator<DeleteRecipe.Command> _commandvalidator;

        public DeleteRecipeTests()
        {
            _commandvalidator = new DeleteRecipe.Validator();
        }

        public void Fail_on_invalid_id()
        {
            var cmd = new DeleteRecipe.Command(-1);

            _commandvalidator.Validate(cmd).IsValid.ShouldBeFalse();
        }

        public void Success_on_valid_id()
        {
            var cmd = new DeleteRecipe.Command(0);

            _commandvalidator.Validate(cmd).IsValid.ShouldBeTrue();
        }
    }
}
