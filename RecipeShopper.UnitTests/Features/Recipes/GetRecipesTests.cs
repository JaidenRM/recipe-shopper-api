using FluentValidation;
using RecipeShopper.Features.Recipes;
using Shouldly;
using Xunit;

namespace RecipeShopper.UnitTests.Features.Recipes
{
    public class GetRecipesTests
    {
        private readonly AbstractValidator<GetRecipes.Query> _queryValidator;

        public GetRecipesTests()
        {
            _queryValidator = new GetRecipes.Validator();
        }

        [Fact]
        public void Reject_on_null_ids()
        {
            var query = new GetRecipes.Query(null);

            _queryValidator.Validate(query).IsValid.ShouldBeFalse();
        }
    }
}
