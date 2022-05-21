using RecipeShopper.Application.Exceptions;
using RecipeShopper.Application.Extensions;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Features.Recipes;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RecipeShopper.IntegrationTests.Features.Recipes
{
    [Collection(nameof(VerticalSliceFixture))]
    public class UpdateRecipeTests
    {
        private readonly VerticalSliceFixture _fixture;

        public UpdateRecipeTests(VerticalSliceFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Should_update_all()
        {
            var recipe = await CreateAndGetGrilledCheeseCreateCommand();
            var updateCmd = CreateUpdateCommandForMilkshake(recipe.Id);

            // ensure we have time difference between the two
            await Task.Delay(3000);

            await _fixture.SendAsync(updateCmd);

            var queryUpdatedRecipe = new GetRecipes.Query(new[] { recipe.Id });
            var resp = await _fixture.SendAsync(queryUpdatedRecipe);

            resp.Results.Count.ShouldBe(1);
            var updatedRecipe = resp.Results[0];

            // stay the same
            updatedRecipe.Id.ShouldBe(recipe.Id);
            updatedRecipe.CreatedOnUTC.IsEqualWithin(recipe.CreatedOnUTC, TimeSpan.FromMilliseconds(1)).ShouldBeTrue();

            // should all change
            updatedRecipe.LastModifiedUTC.IsEqualWithin(recipe.LastModifiedUTC, TimeSpan.FromSeconds(1)).ShouldBeFalse();
            updatedRecipe.Name.ShouldNotBe(recipe.Name);
            updatedRecipe.Servings.ShouldNotBe(recipe.Servings);
            updatedRecipe.DurationMinutes.ShouldNotBe(recipe.DurationMinutes);
            updatedRecipe.Tags.ShouldNotBe(recipe.Tags);

            //fully replaced so should be all new ids
            updatedRecipe.Instructions.Count.ShouldNotBe(recipe.Instructions.Count);
            updatedRecipe.Instructions.Select(ui => ui.Id).ShouldNotContain(uiId => recipe.Instructions.Select(i => i.Id).Contains(uiId));

            updatedRecipe.Ingredients.Count.ShouldNotBe(recipe.Ingredients.Count);
            updatedRecipe.Ingredients.Select(ui => ui.Id).ShouldNotContain(uiId => recipe.Ingredients.Select(i => i.Id).Contains(uiId));

            await _fixture.ResetCheckpoint();
        }

        [Fact]
        public async Task Should_update_only_some_recipe()
        {
            var recipe = await CreateAndGetGrilledCheeseCreateCommand();

            var updateIngredients = new List<UpdateRecipe.UpdateIngredient>();
            var updateInstructions = new List<UpdateRecipe.UpdateInstruction>();

            foreach(var ingredient in recipe.Ingredients)
            {
                var updatedProduct = ingredient.LinkingProduct == null ? null : new UpdateRecipe.UpdateProduct(ingredient.LinkingProduct.Id, ingredient.LinkingProduct.SupermarketId, ingredient.LinkingProduct.Name);
                var updatedIngredient = new UpdateRecipe.UpdateIngredient(ingredient.Id, ingredient.Name, ingredient.Quantity, ingredient.MeasurementUnit, updatedProduct);

                updateIngredients.Add(updatedIngredient);
            }

            foreach(var instruction in recipe.Instructions)
            {
                var updatedInstruction = new UpdateRecipe.UpdateInstruction(instruction.Id, instruction.Order, instruction.Description);

                updateInstructions.Add(updatedInstruction);
            }

            var updateCmd = new UpdateRecipe.Command(recipe.Id, recipe.Name + "hahaha", recipe.Description, recipe.Servings + 6, recipe.DurationMinutes, "fake tag", updateIngredients, updateInstructions);

            await _fixture.SendAsync(updateCmd);

            var queryUpdatedRecipe = new GetRecipes.Query(new[] { recipe.Id });
            var resp = await _fixture.SendAsync(queryUpdatedRecipe);

            resp.Results.Count.ShouldBe(1);
            var updatedRecipe = resp.Results[0];

            updatedRecipe.Id.ShouldBe(recipe.Id);
            updatedRecipe.Name.ShouldNotBe(recipe.Name);
            updatedRecipe.Description.ShouldBe(recipe.Description);
            updatedRecipe.Tags.ShouldNotBe(recipe.Tags);
            updatedRecipe.DurationMinutes.ShouldBe(recipe.DurationMinutes);
            updatedRecipe.Servings.ShouldNotBe(recipe.Servings);
            updatedRecipe.Ingredients.ShouldBeEquivalentTo(recipe.Ingredients);
            updatedRecipe.Instructions.ShouldBeEquivalentTo(recipe.Instructions);

            await _fixture.ResetCheckpoint();
        }

        [Fact]
        public async Task Should_update_only_some_ingredients()
        {
            var recipe = await CreateAndGetGrilledCheeseCreateCommand();

            var updateIngredients = new List<UpdateRecipe.UpdateIngredient>();
            var updateInstructions = new List<UpdateRecipe.UpdateInstruction>();

            var firstIngredientId = recipe.Ingredients[0].Id;
            var secondIngredientId = recipe.Ingredients[1].Id;
            var newIngredient = new UpdateRecipe.UpdateIngredient(null, "Apple", 4, MeasurementUnit.Each.ToFriendlyString(), new UpdateRecipe.UpdateProduct(37909, 1, "Nice name"));

            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                var ingredient = recipe.Ingredients[i];
                UpdateRecipe.UpdateIngredient? updatedIngredient = null;
                // keep same
                if (i == 0)
                {
                    var updatedProduct = ingredient.LinkingProduct == null ? null : new UpdateRecipe.UpdateProduct(ingredient.LinkingProduct.Id, ingredient.LinkingProduct.SupermarketId, ingredient.LinkingProduct.Name);
                    updatedIngredient = new UpdateRecipe.UpdateIngredient(ingredient.Id, ingredient.Name, ingredient.Quantity, ingredient.MeasurementUnit, updatedProduct);
                } // modify
                else if (i == 1)
                {
                    var updatedProduct = new UpdateRecipe.UpdateProduct(444, 1, "lol");
                    updatedIngredient = new UpdateRecipe.UpdateIngredient(ingredient.Id, ingredient.Name + "XD!", ingredient.Quantity, ChooseDifferentUnit(ingredient.MeasurementUnit), updatedProduct);
                } // add new
                else if (i == 2)
                {
                    updatedIngredient = newIngredient;
                }// delete remaining
                else
                {

                }

                if (updatedIngredient != null) updateIngredients.Add(updatedIngredient);
            }

            foreach (var instruction in recipe.Instructions)
            {
                var updatedInstruction = new UpdateRecipe.UpdateInstruction(instruction.Id, instruction.Order, instruction.Description);

                updateInstructions.Add(updatedInstruction);
            }

            var updateCmd = new UpdateRecipe.Command(recipe.Id, recipe.Name, recipe.Description, recipe.Servings, recipe.DurationMinutes, recipe.Tags, updateIngredients, updateInstructions);

            await _fixture.SendAsync(updateCmd);

            var queryUpdatedRecipe = new GetRecipes.Query(new[] { recipe.Id });
            var resp = await _fixture.SendAsync(queryUpdatedRecipe);

            resp.Results.Count.ShouldBe(1);
            var updatedRecipe = resp.Results[0];

            updatedRecipe.Id.ShouldBe(recipe.Id);
            updatedRecipe.Name.ShouldBe(recipe.Name);
            updatedRecipe.Description.ShouldBe(recipe.Description);
            updatedRecipe.Tags.ShouldBe(recipe.Tags);
            updatedRecipe.DurationMinutes.ShouldBe(recipe.DurationMinutes);
            updatedRecipe.Servings.ShouldBe(recipe.Servings);
            updatedRecipe.Instructions.ShouldBeEquivalentTo(recipe.Instructions);

            // first should be same
            var matchingFirstIngredient = recipe.Ingredients.Single(i => i.Id == firstIngredientId);
            var updatedFirstIngredient = updatedRecipe.Ingredients.Single(i => i.Id == firstIngredientId);

            updatedFirstIngredient.ShouldBeEquivalentTo(matchingFirstIngredient);
            // second should have modifications
            var originalSecondIngredient = recipe.Ingredients.Single(i => i.Id == secondIngredientId);
            var updateCommandSecondIngredient = updateIngredients.Single(i => i.Id == secondIngredientId);
            var updatedSecondIngredient = updatedRecipe.Ingredients.Single(i => i.Id == secondIngredientId);

            updatedSecondIngredient.Id.ShouldBe(originalSecondIngredient.Id);
            updatedSecondIngredient.Name.ShouldBe(updateCommandSecondIngredient.Name);
            updatedSecondIngredient.Quantity.ShouldBe(originalSecondIngredient.Quantity);
            updatedSecondIngredient.MeasurementUnit.ShouldBe(updateCommandSecondIngredient.MeasurementUnit);
            
            if (updatedSecondIngredient.LinkingProduct == null)
            {
                updatedSecondIngredient.ShouldBeNull();
            }
            else
            {
                updatedSecondIngredient.LinkingProduct.Id.ShouldBe(updateCommandSecondIngredient.LinkingProduct.Id);
                updatedSecondIngredient.LinkingProduct.SupermarketId.ShouldBe(updateCommandSecondIngredient.LinkingProduct.SupermarketId);
                updatedSecondIngredient.LinkingProduct.Name.ShouldBe(updateCommandSecondIngredient.LinkingProduct.Name);
            }

            // third should be new
            var updatedNewIngredient = updatedRecipe.Ingredients.Single(i => !new[] { firstIngredientId, secondIngredientId }.Contains(i.Id));

            recipe.Ingredients.Select(x => x.Id).ShouldNotContain(updatedNewIngredient.Id);
            updatedNewIngredient.Name.ShouldBe(newIngredient.Name);
            updatedNewIngredient.Quantity.ShouldBe(newIngredient.Quantity);
            updatedNewIngredient.MeasurementUnit.ShouldBe(newIngredient.MeasurementUnit);

            if (updatedNewIngredient.LinkingProduct == null)
            {
                updatedNewIngredient.ShouldBeNull();
            }
            else
            {
                updatedNewIngredient.LinkingProduct.Id.ShouldBe(newIngredient.LinkingProduct.Id);
                updatedNewIngredient.LinkingProduct.SupermarketId.ShouldBe(newIngredient.LinkingProduct.SupermarketId);
                updatedNewIngredient.LinkingProduct.Name.ShouldBe(newIngredient.LinkingProduct.Name);
            }

            // remaining ingredients should have been deleted
            var ingredientIds = updatedRecipe.Ingredients.Select(i => i.Id).ToArray();
            var deletedIds = recipe.Ingredients.Skip(2).Select(i => i.Id).ToArray();

            ingredientIds.ShouldContain(matchingFirstIngredient.Id);
            ingredientIds.ShouldContain(originalSecondIngredient.Id);
            ingredientIds.Any(id => deletedIds.Contains(id)).ShouldBeFalse();

            await _fixture.ResetCheckpoint();
        }

        [Fact]
        public async Task Should_update_only_instructions()
        {
            var recipe = await CreateAndGetGrilledCheeseCreateCommand();

            var updateIngredients = new List<UpdateRecipe.UpdateIngredient>();
            var updateInstructions = new List<UpdateRecipe.UpdateInstruction>();

            var firstInstructionId = recipe.Instructions[0].Id;
            var secondInstructionId = recipe.Instructions[1].Id;
            var newInstruction = new UpdateRecipe.UpdateInstruction(null, 8, "I am the eight order!");

            for (int i = 0; i < recipe.Instructions.Count; i++)
            {
                var instruction = recipe.Instructions[i];
                UpdateRecipe.UpdateInstruction? updateInstruction = null;
                // keep same
                if (i == 0)
                {
                    updateInstruction = new UpdateRecipe.UpdateInstruction(instruction.Id, instruction.Order, instruction.Description);
                } // modify
                else if (i == 1)
                {
                    updateInstruction = new UpdateRecipe.UpdateInstruction(instruction.Id, instruction.Order + 1, instruction.Description);
                } // add new
                else if (i == 2)
                {
                    updateInstruction = newInstruction;
                }// delete remaining
                else
                {

                }

                if (updateInstruction != null) updateInstructions.Add(updateInstruction);
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                var updatedProduct = ingredient.LinkingProduct == null ? null : new UpdateRecipe.UpdateProduct(ingredient.LinkingProduct.Id, ingredient.LinkingProduct.SupermarketId, ingredient.LinkingProduct.Name);
                var updatedIngredient = new UpdateRecipe.UpdateIngredient(ingredient.Id, ingredient.Name, ingredient.Quantity, ingredient.MeasurementUnit, updatedProduct);

                updateIngredients.Add(updatedIngredient);
            }

            var updateCmd = new UpdateRecipe.Command(recipe.Id, recipe.Name, recipe.Description, recipe.Servings, recipe.DurationMinutes, recipe.Tags, updateIngredients, updateInstructions);

            await _fixture.SendAsync(updateCmd);

            var queryUpdatedRecipe = new GetRecipes.Query(new[] { recipe.Id });
            var resp = await _fixture.SendAsync(queryUpdatedRecipe);

            resp.Results.Count.ShouldBe(1);
            var updatedRecipe = resp.Results[0];

            updatedRecipe.Id.ShouldBe(recipe.Id);
            updatedRecipe.Name.ShouldBe(recipe.Name);
            updatedRecipe.Description.ShouldBe(recipe.Description);
            updatedRecipe.Tags.ShouldBe(recipe.Tags);
            updatedRecipe.DurationMinutes.ShouldBe(recipe.DurationMinutes);
            updatedRecipe.Servings.ShouldBe(recipe.Servings);
            updatedRecipe.Ingredients.ShouldBeEquivalentTo(recipe.Ingredients);

            // first should be same
            var matchingFirstInstruction = recipe.Instructions.Single(i => i.Id == firstInstructionId);
            var updatedFirstInstruction = recipe.Instructions.Single(i => i.Id == firstInstructionId);

            updatedFirstInstruction.ShouldBeEquivalentTo(matchingFirstInstruction);
            // second should have modifications
            var originalSecondInstruction = recipe.Instructions.Single(i => i.Id == secondInstructionId);
            var updateCommandSecondInstruction = updateInstructions.Single(i => i.Id == secondInstructionId);
            var updatedSecondInstruction = updatedRecipe.Instructions.Single(i => i.Id == secondInstructionId);

            updatedSecondInstruction.Id.ShouldBe(originalSecondInstruction.Id);
            updatedSecondInstruction.Order.ShouldBe(updateCommandSecondInstruction.Order);
            updatedSecondInstruction.Description.ShouldBe(originalSecondInstruction.Description);

            // third should be new
            var updatedNewInstruction = updatedRecipe.Instructions.Single(i => !new[] { firstInstructionId, secondInstructionId }.Contains(i.Id));

            recipe.Instructions.Select(x => x.Id).ShouldNotContain(updatedNewInstruction.Id);
            updatedNewInstruction.Order.ShouldBe(newInstruction.Order);
            updatedNewInstruction.Description.ShouldBe(newInstruction.Description);

            // remaining ingredients should have been deleted
            var instructionIds = updatedRecipe.Instructions.Select(i => i.Id).ToArray();
            var deletedIds = recipe.Instructions.Skip(2).Select(i => i.Id).ToArray();

            instructionIds.ShouldContain(matchingFirstInstruction.Id);
            instructionIds.ShouldContain(originalSecondInstruction.Id);
            instructionIds.Any(id => deletedIds.Contains(id)).ShouldBeFalse();

            await _fixture.ResetCheckpoint();
        }

        [Fact]
        public async Task Throw_if_recipe_does_not_exist()
        {
            var updateCmd = CreateUpdateCommandForMilkshake(1);
            var sendCmd = async () => await _fixture.SendAsync(updateCmd);

            await sendCmd.ShouldThrowAsync<RecordNotFoundException>();
        }

        #region Helpers
        private async Task<GetRecipes.Model> CreateAndGetGrilledCheeseCreateCommand()
        {
            var rnd = new Random();
            var ingredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Sliced Cheese", 2, MeasurementUnit.Each.ToFriendlyString(), new CreateRecipe.CreateProduct(234567, (int)SupermarketType.Woolworths, "Cheese")),
                new CreateRecipe.CreateIngredient("Butter/Margarine", 1, MeasurementUnit.None.ToFriendlyString(), null),
                new CreateRecipe.CreateIngredient("Sliced Bread", 2, MeasurementUnit.Each.ToFriendlyString(), new CreateRecipe.CreateProduct(985446, (int)SupermarketType.Woolworths, "White Bread")),
            };
            var instructions = new List<CreateRecipe.CreateInstruction>()
            {
                new CreateRecipe.CreateInstruction(0, "Preheat sandwich press (or whatever you're using if needed)"),
                new CreateRecipe.CreateInstruction(1, "Butter both sides of each slice of bread"),
                new CreateRecipe.CreateInstruction(2, "Place the two slices of cheese in between the bread"),
                new CreateRecipe.CreateInstruction(3, "Toast the sandwich until golden brown"),
            };

            var createCmd = new CreateRecipe.Command("Grilled Cheese", "A simple grilled cheese recipe", 1, 10, "Quick,Cheese,Sandwich", ingredients, instructions);
            var recipeId = await _fixture.SendAsync(createCmd);

            var query = new GetRecipes.Query(new[] { recipeId });
            var resp = await _fixture.SendAsync(query);

            resp.Results.Count.ShouldBe(1);
            return resp.Results[0];
        }

        private UpdateRecipe.Command CreateUpdateCommandForMilkshake(int recipeIdToUpdate)
        {
            var updatedIngredients = new List<UpdateRecipe.UpdateIngredient>()
            {
                new UpdateRecipe.UpdateIngredient(null, "Chocolate Syrup", 4, MeasurementUnit.Tablespoon.ToFriendlyString(), null),
                new UpdateRecipe.UpdateIngredient(null, "Chocolate Powder", 4, MeasurementUnit.Tablespoon.ToFriendlyString(), null),
                new UpdateRecipe.UpdateIngredient(null, "Vanilla Ice Cream", 3, MeasurementUnit.Cup.ToFriendlyString(), null),
                new UpdateRecipe.UpdateIngredient(null, "Milk", 2, MeasurementUnit.Cup.ToFriendlyString(), null)
            };
            var updatedInstructions = new List<UpdateRecipe.UpdateInstruction>()
            {
                new UpdateRecipe.UpdateInstruction(null, 1, "Put all ingredients in a blender"),
                new UpdateRecipe.UpdateInstruction(null, 2, "Blend until smooth and enjoy")
            };

            return new UpdateRecipe.Command(recipeIdToUpdate, "My Secret Milkshake Recipe", "It tastes too good to be true!", 2, 5, "Milkshake,Secret", updatedIngredients, updatedInstructions);
        }

        private string ChooseDifferentUnit(string unit)
        {
            var parsedUnit = unit.ToEnum<MeasurementUnit>();
            var allUnits = Enum.GetValues<MeasurementUnit>();

            if (allUnits.Length < 2) throw new InvalidOperationException("Need at least 2 units to allow for a different unit");

            var index = 0;
            var randomUnit = allUnits[index];

            while (randomUnit != parsedUnit)
            {
                index++;
                randomUnit = allUnits[index];
            }

            return randomUnit.ToFriendlyString();
        }
        #endregion
    }
}
