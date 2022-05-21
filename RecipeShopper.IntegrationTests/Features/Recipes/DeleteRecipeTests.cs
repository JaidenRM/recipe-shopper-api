using RecipeShopper.Domain.Enums;
using RecipeShopper.Entities;
using RecipeShopper.Features.Recipes;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RecipeShopper.IntegrationTests.Features.Recipes
{
    [Collection(nameof(VerticalSliceFixture))]
    public class DeleteRecipeTests
    {
        private readonly VerticalSliceFixture _fixture;

        public DeleteRecipeTests(VerticalSliceFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Successfully_delete_recipe()
        {
            var ingredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Strawberries", 0.75m, MeasurementUnit.Cup.ToFriendlyString(), new CreateRecipe.CreateProduct(298967, (int)SupermarketType.Woolworths, "Strawberries")),
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, MeasurementUnit.Tablespoon.ToFriendlyString(), null),
                new CreateRecipe.CreateIngredient("Protein Powder (1 scoop if desired; Vanilla/Raw)", 25, MeasurementUnit.Grams.ToFriendlyString(), null),
                new CreateRecipe.CreateIngredient("Coconut Water", 1, MeasurementUnit.Cup.ToFriendlyString(), new CreateRecipe.CreateProduct(23457889, (int)SupermarketType.Woolworths, "H20 Coconut Water")),
            };
            var instructions = new List<CreateRecipe.CreateInstruction>()
            {
                new CreateRecipe.CreateInstruction(0, "Place all the fruits in the blender"),
                new CreateRecipe.CreateInstruction(1, "Add the coconut water and protein powder if you are using"),
                new CreateRecipe.CreateInstruction(2, "Blend for a minute or so until smooth"),
            };

            var createCmd = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", ingredients, instructions);

            var recipeId = await _fixture.SendAsync(createCmd);

            var query = new GetRecipes.Query(new[] { recipeId });
            var resp = await _fixture.SendAsync(query);

            resp.Results.Count.ShouldBe(1);

            var recipe = resp.Results[0];
            await AssertRecipeComponentsInDb(recipe, true);

            var delCmd = new DeleteRecipe.Command(recipe.Id);
            await _fixture.SendAsync(delCmd);

            await AssertRecipeComponentsInDb(recipe, false);
            await _fixture.ResetCheckpoint();
        }

        [Fact]
        public async Task Successfully_delete_one_recipe_from_collection()
        {
            var ingredients = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Strawberries", 0.75m, MeasurementUnit.Cup.ToFriendlyString(), new CreateRecipe.CreateProduct(298967, (int)SupermarketType.Woolworths, "Strawberries")),
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, MeasurementUnit.Tablespoon.ToFriendlyString(), null),
                new CreateRecipe.CreateIngredient("Protein Powder (1 scoop if desired; Vanilla/Raw)", 25, MeasurementUnit.Grams.ToFriendlyString(), null),
                new CreateRecipe.CreateIngredient("Coconut Water", 1, MeasurementUnit.Cup.ToFriendlyString(), new CreateRecipe.CreateProduct(23457889, (int)SupermarketType.Woolworths, "H20 Coconut Water")),
            };
            var instructions = new List<CreateRecipe.CreateInstruction>()
            {
                new CreateRecipe.CreateInstruction(0, "Place all the fruits in the blender"),
                new CreateRecipe.CreateInstruction(1, "Add the coconut water and protein powder if you are using"),
                new CreateRecipe.CreateInstruction(2, "Blend for a minute or so until smooth"),
            };

            var createCmd1 = new CreateRecipe.Command("Smoothie",
                "Here is my recipe to a super revolutionary smoothie that will change your life!!",
                1, 7, "Healthy,Liquid,Quick", ingredients, instructions);

            var ingredients2 = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Chicken Thigh", 600, MeasurementUnit.Grams.ToFriendlyString(), new CreateRecipe.CreateProduct(255790, (int)SupermarketType.Woolworths, "Strawberries")),
                new CreateRecipe.CreateIngredient("Peanut Butter", 2, MeasurementUnit.Tablespoon.ToFriendlyString(), null),
                new CreateRecipe.CreateIngredient("Satay", 250, MeasurementUnit.Grams.ToFriendlyString(), null),
                new CreateRecipe.CreateIngredient("Coconut Milk", 1, MeasurementUnit.Cup.ToFriendlyString(), new CreateRecipe.CreateProduct(345367, (int)SupermarketType.Woolworths, "H20 Coconut Water")),
                new CreateRecipe.CreateIngredient("Thai Red Curry Paste", 1, MeasurementUnit.Tablespoon.ToFriendlyString(), new CreateRecipe.CreateProduct(43379, (int)SupermarketType.Woolworths, "Thai Red Curry Paste")),
                new CreateRecipe.CreateIngredient("Curry Powder", 1, MeasurementUnit.Teaspoon.ToFriendlyString(), null),
            };
            var instructions2 = new List<CreateRecipe.CreateInstruction>()
            {
                new CreateRecipe.CreateInstruction(0, "Cut the chicken into 2cm cubes"),
                new CreateRecipe.CreateInstruction(1, "Combine chicken, coconut milk, curry powder and curry paste together in a bowl. Allow it to sit for at least 20 minutes"),
                new CreateRecipe.CreateInstruction(2, "Partially cook chicken until all seared"),
                new CreateRecipe.CreateInstruction(3, "Combine satay, peanut butter and chicken and cook on medium-low until chicken is cooked through"),
            };

            var createCmd2 = new CreateRecipe.Command("Chicken Satay", "Easy and delicious", 4, 40, "Easy,Peanut,Chicken", ingredients2, instructions2);

            var ingredients3 = new List<CreateRecipe.CreateIngredient>()
            {
                new CreateRecipe.CreateIngredient("Strawberries", 1, MeasurementUnit.Cup.ToFriendlyString(), new CreateRecipe.CreateProduct(96574, (int)SupermarketType.Woolworths, "Strawberries")),
                new CreateRecipe.CreateIngredient("Water", 0.5m, MeasurementUnit.Cup.ToFriendlyString(), null),
                new CreateRecipe.CreateIngredient("Sugar", 0.5m, MeasurementUnit.Cup.ToFriendlyString(), new CreateRecipe.CreateProduct(227898, (int)SupermarketType.Woolworths, "White Sugar")),
            };
            var instructions3 = new List<CreateRecipe.CreateInstruction>()
            {
                new CreateRecipe.CreateInstruction(0, "Place water and sugar in a saucepan. Bring to boil"),
                new CreateRecipe.CreateInstruction(1, "Cut strawberries into little pieces and place in boiling water. Stir and crush strawberries into the mixture"),
                new CreateRecipe.CreateInstruction(2, "Simmer mixture for 20-30 minutes until thickened"),
            };

            var createCmd3 = new CreateRecipe.Command("Strawberry Jam", "Super simple jam recipe that may or may not work", 20, 35, "Jam,Simple,Minimal", ingredients3, instructions3);

            var id1 = await _fixture.SendAsync(createCmd1);
            var id2 = await _fixture.SendAsync(createCmd2);
            var id3 = await _fixture.SendAsync(createCmd3);

            var query = new GetRecipes.Query(new[] { id1, id2, id3 });
            var resp = await _fixture.SendAsync(query);

            resp.Results.Count.ShouldBe(3);
            var recipe = resp.Results.Single(r => r.Id == id2);

            await AssertRecipeComponentsInDb(recipe, true);

            var delCmd = new DeleteRecipe.Command(id2);
            await _fixture.SendAsync(delCmd);

            await AssertRecipeComponentsInDb(recipe, false);
            await _fixture.ResetCheckpoint();
        }

        #region Helpers
        private async Task AssertRecipeComponentsInDb(GetRecipes.Model recipe, bool shouldBeInDb)
        {
            foreach(var ingredient in recipe.Ingredients)
            {
                var dbIng = await _fixture.FindAsync<Ingredient>(ingredient.Id);
                AssertIsOrIsNotNull(dbIng, !shouldBeInDb);

                if (ingredient.LinkingProduct != null)
                {
                    var dbProd = await _fixture.FindAsync<Product>(ingredient.LinkingProduct.Id, ingredient.LinkingProduct.SupermarketId);
                    AssertIsOrIsNotNull(dbProd, !shouldBeInDb);
                }
            }

            foreach (var instruction in recipe.Instructions)
            {
                var dbInst = await _fixture.FindAsync<Instruction>(instruction.Id);
                AssertIsOrIsNotNull(dbInst, !shouldBeInDb);
            }
        }

        private void AssertIsOrIsNotNull(object obj, bool assertIsNull)
        {
            if (assertIsNull) obj.ShouldBeNull();
            else obj.ShouldNotBeNull();
        }
        #endregion
    }
}
