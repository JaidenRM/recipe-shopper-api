using Xunit;
using Shouldly;
using System.Threading.Tasks;
using RecipeShopper.Entities;
using System;
using System.Collections.Generic;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Features.Recipes;
using System.Linq;
using RecipeShopper.Application.Extensions;

namespace RecipeShopper.IntegrationTests.Features.Recipes
{
    [Collection(nameof(VerticalSliceFixture))]
    public class GetRecipesTests
    {
        private readonly VerticalSliceFixture _fixture;

        public GetRecipesTests(VerticalSliceFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Should_get_all_recipes()
        {
            #region Test Data
            var now = DateTime.UtcNow;
            var recipe1 = new Recipe
            {
                Name = "Recipe 1",
                Description = "This is my first recipe",
                Servings = 5,
                DurationMinutes = 30,
                CreatedOnUTC = now,
                LastModifiedUTC = now,
                Tags = "Easy,Fun,Beginner Friendly",
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient
                    {
                        Name = "Carrots",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                    },
                    new Ingredient
                    {
                        Name = "Potatoes",
                        Quantity = 3,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 123456,
                            Name = "Potato",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    }
                },
                Instructions = new List<Instruction>()
                {
                    new Instruction
                    {
                        Description = "Prepare vegetables",
                        Order = 1,
                    },
                    new Instruction
                    {
                        Description = "Cook vegetables in boiling water until soft. About 10-15 mins.",
                        Order = 2,
                    },
                    new Instruction
                    {
                        Description = "Drain water and enjoy!",
                        Order = 3
                    }
                }
            };
            var recipe2 = new Recipe
            {
                Name = "Recipe 2",
                Description = "Good soup",
                Servings = 2,
                DurationMinutes = 10,
                CreatedOnUTC = now,
                LastModifiedUTC = now,
                Tags = "Easy,Soup",
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient
                    {
                        Name = "Carrots",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                    },
                    new Ingredient
                    {
                        Name = "Celery",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 5346,
                            Name = "Celery",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    },
                    new Ingredient
                    {
                        Name = "Vegetable Stock",
                        Quantity = 500,
                        Unit = MeasurementUnit.Millilitres,
                        Product = new Product
                        {
                            Id = 845425,
                            Name = "Stock - Vegetable",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    }
                },
                Instructions = new List<Instruction>()
                {
                    new Instruction
                    {
                        Description = "Prepare vegetables",
                        Order = 1,
                    },
                    new Instruction
                    {
                        Description = "Cook until boiling and then simmer until vegetables are tender",
                        Order = 3,
                    },
                    new Instruction
                    {
                        Description = "Add stock into a pot and combine with vegetables",
                        Order = 2
                    },
                    new Instruction
                    {
                        Description = "Allow soup to cool a little bit then portion out into bowls for serving",
                        Order = 4
                    }
                }
            };
            var recipe3 = new Recipe
            {
                Name = "Toast",
                Description = "Very difficult!",
                Servings = 1,
                DurationMinutes = 5,
                CreatedOnUTC = now,
                LastModifiedUTC = now,
                Tags = "Difficult,Bread,Hot",
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient
                    {
                        Name = "Sliced bread",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 423566,
                            Name = "Toast White Bread",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    },
                    new Ingredient
                    {
                        Name = "Butter (if you want to spread it on the bread)",
                        Quantity = 1,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 65344,
                            Name = "Western Star Butter",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    },
                    new Ingredient
                    {
                        Name = "Any spread you want on your toast",
                        Quantity = 1,
                        Unit = MeasurementUnit.Each
                    }
                },
                Instructions = new List<Instruction>()
                {
                    new Instruction
                    {
                        Description = "Slot bread into the toaster",
                        Order = 1,
                    },
                    new Instruction
                    {
                        Description = "Set your desired 'toasty-ness' and start toasting",
                        Order = 2,
                    },
                    new Instruction
                    {
                        Description = "Once toasting is finished, place bread on a plate and start spreading butter and/or any other spreads you want",
                        Order = 3
                    }
                }
            };
            #endregion

            var recipes = new[] { recipe1, recipe2, recipe3 };
            await _fixture.InsertAsync(recipes);

            var query = new GetRecipes.Query(recipes.Select(r => r.Id).ToArray());
            var resp = await _fixture.SendAsync(query);

            resp.Results.Count.ShouldBe(recipes.Length);

            foreach (var recipe in resp.Results)
            {
                var matchingRecipe = recipes.Single(r => r.Id == recipe.Id);
                RecipesShouldBeEqual(matchingRecipe, recipe);
            }

            await _fixture.ResetCheckpoint();
        }

        [Fact]
        public async Task Should_get_specific_recipes()
        {
            #region Test Data
            var now = DateTime.UtcNow;
            var recipe1 = new Recipe
            {
                Name = "Recipe 1",
                Description = "This is my first recipe",
                Servings = 5,
                DurationMinutes = 30,
                CreatedOnUTC = now,
                LastModifiedUTC = now,
                Tags = "Easy,Fun,Beginner Friendly",
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient
                    {
                        Name = "Carrots",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                    },
                    new Ingredient
                    {
                        Name = "Potatoes",
                        Quantity = 3,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 123456,
                            Name = "Potato",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    }
                },
                Instructions = new List<Instruction>()
                {
                    new Instruction
                    {
                        Description = "Prepare vegetables",
                        Order = 1,
                    },
                    new Instruction
                    {
                        Description = "Cook vegetables in boiling water until soft. About 10-15 mins.",
                        Order = 2,
                    },
                    new Instruction
                    {
                        Description = "Drain water and enjoy!",
                        Order = 3
                    }
                }
            };
            var recipe2 = new Recipe
            {
                Name = "Recipe 2",
                Description = "Good soup",
                Servings = 2,
                DurationMinutes = 10,
                CreatedOnUTC = now,
                LastModifiedUTC = now,
                Tags = "Easy,Soup",
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient
                    {
                        Name = "Carrots",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                    },
                    new Ingredient
                    {
                        Name = "Celery",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 5346,
                            Name = "Celery",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    },
                    new Ingredient
                    {
                        Name = "Vegetable Stock",
                        Quantity = 500,
                        Unit = MeasurementUnit.Millilitres,
                        Product = new Product
                        {
                            Id = 845425,
                            Name = "Stock - Vegetable",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    }
                },
                Instructions = new List<Instruction>()
                {
                    new Instruction
                    {
                        Description = "Prepare vegetables",
                        Order = 1,
                    },
                    new Instruction
                    {
                        Description = "Cook until boiling and then simmer until vegetables are tender",
                        Order = 3,
                    },
                    new Instruction
                    {
                        Description = "Add stock into a pot and combine with vegetables",
                        Order = 2
                    },
                    new Instruction
                    {
                        Description = "Allow soup to cool a little bit then portion out into bowls for serving",
                        Order = 4
                    }
                }
            };
            var recipe3 = new Recipe
            {
                Name = "Toast",
                Description = "Very difficult!",
                Servings = 1,
                DurationMinutes = 5,
                CreatedOnUTC = now,
                LastModifiedUTC = now,
                Tags = "Difficult,Bread,Hot",
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient
                    {
                        Name = "Sliced bread",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 423566,
                            Name = "Toast White Bread",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    },
                    new Ingredient
                    {
                        Name = "Butter (if you want to spread it on the bread)",
                        Quantity = 1,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 65344,
                            Name = "Western Star Butter",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    },
                    new Ingredient
                    {
                        Name = "Any spread you want on your toast",
                        Quantity = 1,
                        Unit = MeasurementUnit.Each
                    }
                },
                Instructions = new List<Instruction>()
                {
                    new Instruction
                    {
                        Description = "Slot bread into the toaster",
                        Order = 1,
                    },
                    new Instruction
                    {
                        Description = "Set your desired 'toasty-ness' and start toasting",
                        Order = 2,
                    },
                    new Instruction
                    {
                        Description = "Once toasting is finished, place bread on a plate and start spreading butter and/or any other spreads you want",
                        Order = 3
                    }
                }
            };
            #endregion

            var recipes = new[] { recipe1, recipe2, recipe3 };
            await _fixture.InsertAsync(recipes);

            var query = new GetRecipes.Query(new[] { recipe3.Id, recipe2.Id });
            var resp = await _fixture.SendAsync(query);

            resp.Results.Count.ShouldBe(2);

            foreach(var recipe in resp.Results)
            {
                var matchingRecipe = recipes.Single(r => r.Id == recipe.Id);
                RecipesShouldBeEqual(matchingRecipe, recipe);
            }

            await _fixture.ResetCheckpoint();
        }

        [Fact]
        public async Task Should_get_specific_recipe()
        {
            #region Test Data
            var now = DateTime.UtcNow;
            var recipe1 = new Recipe
            {
                Name = "Recipe 1",
                Description = "This is my first recipe",
                Servings = 5,
                DurationMinutes = 30,
                CreatedOnUTC = now,
                LastModifiedUTC = now,
                Tags = "Easy,Fun,Beginner Friendly",
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient
                    {
                        Name = "Carrots",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                    },
                    new Ingredient
                    {
                        Name = "Potatoes",
                        Quantity = 3,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 123456,
                            Name = "Potato",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    }
                },
                Instructions = new List<Instruction>()
                {
                    new Instruction
                    {
                        Description = "Prepare vegetables",
                        Order = 1,
                    },
                    new Instruction
                    {
                        Description = "Cook vegetables in boiling water until soft. About 10-15 mins.",
                        Order = 2,
                    },
                    new Instruction
                    {
                        Description = "Drain water and enjoy!",
                        Order = 3
                    }
                }
            };
            var recipe2 = new Recipe
            {
                Name = "Recipe 2",
                Description = "Good soup",
                Servings = 2,
                DurationMinutes = 10,
                CreatedOnUTC = now,
                LastModifiedUTC = now,
                Tags = "Easy,Soup",
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient
                    {
                        Name = "Carrots",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                    },
                    new Ingredient
                    {
                        Name = "Celery",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 5346,
                            Name = "Celery",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    },
                    new Ingredient
                    {
                        Name = "Vegetable Stock",
                        Quantity = 500,
                        Unit = MeasurementUnit.Millilitres,
                        Product = new Product
                        {
                            Id = 845425,
                            Name = "Stock - Vegetable",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    }
                },
                Instructions = new List<Instruction>()
                {
                    new Instruction
                    {
                        Description = "Prepare all the vegetables",
                        Order = 1,
                    },
                    new Instruction
                    {
                        Description = "Cook until boiling and then simmer until vegetables are tender",
                        Order = 3,
                    },
                    new Instruction
                    {
                        Description = "Add stock into a pot and combine with vegetables",
                        Order = 2
                    },
                    new Instruction
                    {
                        Description = "Allow soup to cool a little bit then portion out into bowls for serving",
                        Order = 4
                    }
                }
            };
            var recipe3 = new Recipe
            {
                Name = "Toast",
                Description = "Very difficult!",
                Servings = 1,
                DurationMinutes = 5,
                CreatedOnUTC = now,
                LastModifiedUTC = now,
                Tags = "Difficult,Bread,Hot",
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient
                    {
                        Name = "Sliced bread",
                        Quantity = 2,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 423566,
                            Name = "Toast White Bread",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    },
                    new Ingredient
                    {
                        Name = "Butter (if you want to spread it on the bread)",
                        Quantity = 1,
                        Unit = MeasurementUnit.Each,
                        Product = new Product
                        {
                            Id = 65344,
                            Name = "Western Star Butter",
                            SupermarketId = (int) SupermarketType.Woolworths,
                        }
                    },
                    new Ingredient
                    {
                        Name = "Any spread you want on your toast",
                        Quantity = 1,
                        Unit = MeasurementUnit.Each
                    }
                },
                Instructions = new List<Instruction>()
                {
                    new Instruction
                    {
                        Description = "Slot bread into the toaster",
                        Order = 1,
                    },
                    new Instruction
                    {
                        Description = "Set your desired 'toasty-ness' and start toasting",
                        Order = 2,
                    },
                    new Instruction
                    {
                        Description = "Once toasting is finished, place bread on a plate and start spreading butter and/or any other spreads you want",
                        Order = 3
                    }
                }
            };
            #endregion

            var recipes = new[] { recipe1, recipe2, recipe3 };
            await _fixture.InsertAsync(recipes);

            var query = new GetRecipes.Query(new[] { recipe1.Id });
            var resp = await _fixture.SendAsync(query);

            resp.Results.Count.ShouldBe(1);
            RecipesShouldBeEqual(recipe1, resp.Results[0]);
        }

        // Small helper to skim through the two recipe classes for a rough check of equality
        private void RecipesShouldBeEqual(Recipe recipe, GetRecipes.Model recipeModel)
        {
            recipeModel.DurationMinutes.ShouldBe(recipe.DurationMinutes);
            recipe.CreatedOnUTC.IsEqualWithin(recipeModel.CreatedOnUTC, TimeSpan.FromMilliseconds(1)).ShouldBe(true);
            recipe.LastModifiedUTC.IsEqualWithin(recipeModel.LastModifiedUTC, TimeSpan.FromMilliseconds(1)).ShouldBe(true);

            recipeModel.Ingredients.Count.ShouldBe(recipe.Ingredients.Count);
            foreach(var ingredient in recipe.Ingredients)
            {
                recipeModel.Ingredients
                    .Select(i => i.Name)
                    .ShouldContain(ingredient.Name);
            }

            recipeModel.Instructions.Count.ShouldBe(recipe.Instructions.Count);
            foreach(var instruction in recipe.Instructions)
            {
                var matchingInstruction = recipeModel.Instructions.Single(i => i.Order == instruction.Order);
                instruction.Description.ShouldBe(matchingInstruction.Description);
            }
        }
    }
}
