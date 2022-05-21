using FluentValidation;
using RecipeShopper.Application.Exceptions;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Features.Recipes;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace RecipeShopper.UnitTests.Features.Recipes
{
    public class CreateRecipeTests
    {
        private readonly AbstractValidator<CreateRecipe.Command> _commandValidator;

        public CreateRecipeTests()
        {
            _commandValidator = new CreateRecipe.Validator();
        }

         [Fact]
        public void Allow_no_ingredients_or_instructions()
        {
            var cmd = new CreateRecipe.Command("Recipe", "This is my recipe", 1, 4, "tag,tag", null, null);

            _commandValidator.Validate(cmd).IsValid.ShouldBe(true);
        }

        #region Instructions
        [Fact]
        public void Allow_sparsely_ordered_instructions()
        {
            var ingredients = new List<CreateRecipe.CreateIngredient>() {};
            var instructions = new List<CreateRecipe.CreateInstruction>()
            {
                new CreateRecipe.CreateInstruction(3, "Place all the fruits in the blender"),
                new CreateRecipe.CreateInstruction(6, "Add the coconut water and protein powder if you are using"),
                new CreateRecipe.CreateInstruction(11, "Blend for a minute or so until smooth"),
            };

            var cmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", ingredients, instructions);

            _commandValidator.Validate(cmd).IsValid.ShouldBe(true);
        }

        [Fact]
        public void Reject_duplicate_instruction_orders()
        {
            var ingredients = new List<CreateRecipe.CreateIngredient>() {};
            var instructions = new List<CreateRecipe.CreateInstruction>()
            {
                new CreateRecipe.CreateInstruction(0, "Place all the fruits in the blender"),
                new CreateRecipe.CreateInstruction(1, "Add the coconut water and protein powder if you are using"),
                new CreateRecipe.CreateInstruction(1, "Blend for a minute or so until smooth"),
            };

            var cmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", ingredients, instructions);

            _commandValidator.Validate(cmd).IsValid.ShouldBe(false);
        }

        [Fact]
        public void Reject_negatively_ordered_instructions()
        {
            var ingredients = new List<CreateRecipe.CreateIngredient>() {};
            var instructions = new List<CreateRecipe.CreateInstruction>()
            {
                new CreateRecipe.CreateInstruction(0, "Place all the fruits in the blender"),
                new CreateRecipe.CreateInstruction(-1, "Add the coconut water and protein powder if you are using"),
                new CreateRecipe.CreateInstruction(1, "Blend for a minute or so until smooth"),
            };

            var cmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", ingredients, instructions);

            _commandValidator.Validate(cmd).IsValid.ShouldBe(false);
        }
        #endregion

        #region Ingredients
        [Fact]
        public void Allow_ingredient_with_null_product()
        {
            var ingredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, MeasurementUnit.Tablespoon.ToFriendlyString(), null)
            };
            var instructions = new List<CreateRecipe.CreateInstruction>() {};

            var cmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", ingredients, instructions);

            _commandValidator.Validate(cmd).IsValid.ShouldBe(true);
        }

        [Fact]
        public void Reject_ingredient_no_name()
        {
            // #1
            var nullIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient(null, 0.75m, MeasurementUnit.Cup.ToFriendlyString(), new CreateRecipe.CreateProduct(234567, (int)SupermarketType.Woolworths, null)),
            };
            var nullInstructions = new List<CreateRecipe.CreateInstruction>() {};

            var nullCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", nullIngredients, nullInstructions);

            _commandValidator.Validate(nullCmd).IsValid.ShouldBe(false);

            // #2
            var emptyStrIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("", 0.75m, MeasurementUnit.Cup.ToFriendlyString(), new CreateRecipe.CreateProduct(234567, (int)SupermarketType.Woolworths, "")),
            };
            var emptyStrInstructions = new List<CreateRecipe.CreateInstruction>() { };

            var emptyStrCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", emptyStrIngredients, emptyStrInstructions);

            _commandValidator.Validate(emptyStrCmd).IsValid.ShouldBe(false);
        }

        [Fact]
        public void Reject_ingredient_empty_quantity()
        {
            // #1
            var zeroQuantityIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Peanut Butter", 0, MeasurementUnit.Tablespoon.ToFriendlyString(), null)
            };
            var zeroQuantityInstructions = new List<CreateRecipe.CreateInstruction>() { };

            var zeroQuantityCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", zeroQuantityIngredients, zeroQuantityInstructions);

            _commandValidator.Validate(zeroQuantityCmd).IsValid.ShouldBe(false);

            // #2
            var minusIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Peanut Butter", -1, MeasurementUnit.Tablespoon.ToFriendlyString(), null)
            };
            var minusInstructions = new List<CreateRecipe.CreateInstruction>() { };

            var minusCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", minusIngredients, minusInstructions);

            _commandValidator.Validate(minusCmd).IsValid.ShouldBe(false);
        }

        [Fact]
        public void Reject_ingredient_invalid_measurement_unit()
        {
            // #1
            var invalidStrIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, "no way this is a measurement unit", null)
            };
            var invalidStrInstructions = new List<CreateRecipe.CreateInstruction>() { };

            var invalidStrCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", invalidStrIngredients, invalidStrInstructions);

            var toValidateInvalidStr = () => _commandValidator.Validate(invalidStrCmd).IsValid.ShouldBe(false);
            toValidateInvalidStr.ShouldThrow<InvalidEnumException>();

            // #2
            var nullIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, "no way this is a measurement unit", null)
            };
            var nullInstructions = new List<CreateRecipe.CreateInstruction>() { };

            var nullCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", nullIngredients, nullInstructions);

            var toValidateNull = () => _commandValidator.Validate(nullCmd).IsValid.ShouldBe(false);
            toValidateNull.ShouldThrow<InvalidEnumException>();
        }

        [Fact]
        public void Reject_ingredient_product_invalid_id()
        {
            // #1
            var zeroProductIdIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, MeasurementUnit.Tablespoon.ToFriendlyString(), 
                    new CreateRecipe.CreateProduct(0, (int)SupermarketType.Woolworths, "Mayver's Peanut Butter"))
            };
            var zeroProductIdInstructions = new List<CreateRecipe.CreateInstruction>() { };

            var zeroProductIdCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", zeroProductIdIngredients, zeroProductIdInstructions);

            _commandValidator.Validate(zeroProductIdCmd).IsValid.ShouldBe(false);

            // #2
            var minusProductIdIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, MeasurementUnit.Tablespoon.ToFriendlyString(), 
                    new CreateRecipe.CreateProduct(-23453, (int)SupermarketType.Woolworths, "Bega Peanut Butter"))
            };
            var minusProductIdInstructions = new List<CreateRecipe.CreateInstruction>() { };

            var minusProductIdCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", minusProductIdIngredients, minusProductIdInstructions);

            _commandValidator.Validate(minusProductIdCmd).IsValid.ShouldBe(false);
        }

        [Fact]
        public void Reject_ingredient_product_invalid_supermarket_id()
        {
            // #1
            var zeroSupermarketIdIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, MeasurementUnit.Tablespoon.ToFriendlyString(),
                    new CreateRecipe.CreateProduct(52464, 0, "Mayver's Roasted Peanut Butter"))
            };
            var zeroSupermarketIdInstructions = new List<CreateRecipe.CreateInstruction>() { };

            var zeroSupermarketIdCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", zeroSupermarketIdIngredients, zeroSupermarketIdInstructions);

            _commandValidator.Validate(zeroSupermarketIdCmd).IsValid.ShouldBe(false);

            // #2
            var minusSupermarketIdIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, MeasurementUnit.Tablespoon.ToFriendlyString(),
                    new CreateRecipe.CreateProduct(52464, -432, "Peanut Butter"))
            };
            var minusSupermarketIdInstructions = new List<CreateRecipe.CreateInstruction>() { };

            var minusSupermarketIdCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", minusSupermarketIdIngredients, minusSupermarketIdInstructions);

            _commandValidator.Validate(minusSupermarketIdCmd).IsValid.ShouldBe(false);
        }

        [Fact]
        public void Reject_ingredient_product_no_supermarket_name()
        {
            // #1
            var nullSupermarketNameIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, MeasurementUnit.Tablespoon.ToFriendlyString(),
                    new CreateRecipe.CreateProduct(52464, (int)SupermarketType.Woolworths, null))
            };
            var nullSupermarketNameInstructions = new List<CreateRecipe.CreateInstruction>() { };

            var nullSupermarketNameCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", nullSupermarketNameIngredients, nullSupermarketNameInstructions);

            _commandValidator.Validate(nullSupermarketNameCmd).IsValid.ShouldBe(false);

            // #2
            var emptySupermarketNameIngredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, MeasurementUnit.Tablespoon.ToFriendlyString(),
                    new CreateRecipe.CreateProduct(52464, (int)SupermarketType.Woolworths, ""))
            };
            var emptySupermarketNameInstructions = new List<CreateRecipe.CreateInstruction>() { };

            var emptySupermarketNameCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", emptySupermarketNameIngredients, emptySupermarketNameInstructions);

            _commandValidator.Validate(emptySupermarketNameCmd).IsValid.ShouldBe(false);
        }
        #endregion
    }
}
