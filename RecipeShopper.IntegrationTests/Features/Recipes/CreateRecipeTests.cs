using RecipeShopper.Application.Extensions;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Entities;
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
    public class CreateRecipeTests
    {
        private readonly VerticalSliceFixture _fixture;

        public CreateRecipeTests(VerticalSliceFixture fixture) => _fixture = fixture;
        
        [Fact]
        public async Task Should_create_recipe()
        {
            var ingredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Strawberries", 0.75m, MeasurementUnit.Cup.ToFriendlyString(), new CreateRecipe.CreateProduct(234567, (int)SupermarketType.Woolworths, "Strawberries", new CreateRecipe.CreateImageSet("small", "medium", "large"))),
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, MeasurementUnit.Tablespoon.ToFriendlyString(), null),
                new CreateRecipe.CreateIngredient("Protein Powder (1 scoop if desired; Vanilla/Raw)", 25, MeasurementUnit.Grams.ToFriendlyString(), null),
                new CreateRecipe.CreateIngredient("Coconut Water", 1, MeasurementUnit.Cup.ToFriendlyString(), new CreateRecipe.CreateProduct(368749, (int)SupermarketType.Woolworths, "H20 Coconut Water", new CreateRecipe.CreateImageSet("smallWater", "mediumWater", "largeWater"))),
            };
            var instructions = new List<CreateRecipe.CreateInstruction>()
            {
                new CreateRecipe.CreateInstruction(0, "Place all the fruits in the blender"),
                new CreateRecipe.CreateInstruction(1, "Add the coconut water and protein powder if you are using"),
                new CreateRecipe.CreateInstruction(2, "Blend for a minute or so until smooth"),
            };

            var cmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", ingredients, instructions);

            var recipeId = await _fixture.SendAsync(cmd);

            var query = new GetRecipes.Query(new[] { recipeId });
            var resp = await _fixture.SendAsync(query);

            resp.Results.Count.ShouldBe(1);
            RecipesShouldBeEqual(resp.Results.First(), cmd);
        }

        [Fact]
        public async Task Should_reject_recipe()
        {
            var cmd = new CreateRecipe.Command(null, null, -1, -1, null, null, null);

            var sendCmd = async () => await _fixture.SendAsync(cmd);
            await sendCmd.ShouldThrowAsync<FluentValidation.ValidationException>();
        }

        private void RecipesShouldBeEqual(GetRecipes.Model recipe, CreateRecipe.Command recipeCmd)
        {
            recipeCmd.DurationMinutes.ShouldBe(recipe.DurationMinutes);

            // rough check to see if times were added appropriately
            var now = DateTime.Now;
            recipe.CreatedOnUTC.IsEqualWithin(now, TimeSpan.FromMinutes(1)).ShouldBe(true);
            recipe.LastModifiedUTC.IsEqualWithin(now, TimeSpan.FromMinutes(1)).ShouldBe(true);

            recipeCmd.Ingredients.Count.ShouldBe(recipe.Ingredients.Count);
            foreach (var ingredient in recipe.Ingredients)
            {
                recipeCmd.Ingredients
                    .Select(i => i.Name)
                    .ShouldContain(ingredient.Name);
            }

            recipeCmd.Instructions.Count.ShouldBe(recipe.Instructions.Count);
            foreach (var instruction in recipe.Instructions)
            {
                var matchingInstruction = recipeCmd.Instructions.Single(i => i.Order == instruction.Order);
                instruction.Description.ShouldBe(matchingInstruction.Description);
            }
        }
    }
}
