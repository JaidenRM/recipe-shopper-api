using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecipeShopper.Application.Exceptions;
using RecipeShopper.Application.Extensions;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Entities;

namespace RecipeShopper.Features.Recipes
{
    public class UpdateRecipe
    {
        /// <summary>Properties used to update an existing recipe</summary>
        public record Command(int Id, string Name, string Description, int Servings, int DurationMinutes, string Tags, List<UpdateIngredient> Ingredients, List<UpdateInstruction> Instructions) : ICommand<Unit> 
        {
            /// <summary>Id of the recipe</summary>
            /// <example>42</example>
            public int Id { get; init; } = Id;
            /// <example>Recipe #3</example>
            public string Name { get; init; } = Name;
            /// <example>This is a very old recipe of mine that I've felt like updating.</example>
            public string Description { get; init; } = Description;
            /// <summary>Comma-separated list of ways to categorise your recipe</summary>
            /// <example>Family recipe,Generational,Authentic</example>
            public string Tags { get; init; } = Tags;
            /// <summary>The approximate number of people this recipe would serve</summary>
            /// <example>6</example>
            public int Servings { get; init; } = Servings;
            /// <summary>The total length of time it would take to make this recipe on average</summary>
            /// <example>60</example>
            public int DurationMinutes { get; init; } = DurationMinutes;
        };

        /// <summary>The supermarket representation of the mention ingredient</summary>
        public record UpdateProduct(int Id, int SupermarketId, string Name)
        {
            /// <summary>Represents the id of this product from a specific store</summary>
            /// <example>3568</example>
            public int Id { get; init; } = Id;
            /// <summary>Name of the product from the store</summary>
            /// <example>Mary's Pasta Sauce</example>
            public string Name { get; init; } = Name;
            /// <summary>The internal id used to track which supermarket this product is from</summary>
            /// <example>5</example>
            public int SupermarketId { get; init; } = SupermarketId;
        };

        /// <summary>Contains details of a specific ingredient used in this recipe</summary>
        public record UpdateIngredient(int? Id, string Name, decimal Quantity, string MeasurementUnit, UpdateProduct LinkingProduct)
        {
            /// <example>1</example>
            public int? Id { get; init; } = Id;
            /// <example>Pasta Sauce</example>
            public string Name { get; init; } = Name;
            /// <summary>The amount you want. The unit is determined by `MeasurementUnit`</summary>
            /// <example>700</example>
            public decimal Quantity { get; init; } = Quantity;
            /// <summary>
            ///     This is used to give meaning to `Quantity` so we know how we can measure it. 
            ///     Valid values for this include: `"none"`, `"each"`, `"teaspoon"`, `"tablespoon"`, `"grams"`, `"kilograms"`, `"millilitres"`, `"litres"`, `"cup"`, `"pinch"`
            /// </summary>
            /// <example>Millilitres</example>
            public string MeasurementUnit { get; init; } = MeasurementUnit;
            /// <summary>This is the a specific version of the ingredient from a supermarket</summary>
            public UpdateProduct LinkingProduct { get; init; } = LinkingProduct;
        };
        
        /// <summary>Contains details on one part of how to make this recipe</summary>
        public record UpdateInstruction(int? Id, int Order, string Description)
        {
            /// <example>14</example>
            public int? Id { get; init; } = Id;
            /// <summary>Used to generate the order this instruction will appear. Lowest -> Highest</summary>
            /// <example>5</example>
            public int Order { get; init; } = Order;
            /// <summary>Describes what someone should be doing for this instruction</summary>
            /// <example>Simmer the sauce until warm</example>
            public string Description { get; init; } = Description;
        };

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
                RuleFor(c => c.Name).NotEmpty();
                RuleFor(c => c.Servings).GreaterThan(0);
                RuleFor(c => c.DurationMinutes).GreaterThan(0);
                // No nulls, empty arr [] should be used to signal no instructions/ingredients
                RuleFor(c => c.Ingredients).NotNull();
                RuleFor(c => c.Instructions).NotNull();
                // Shouldn't be any identical `Order` values
                RuleFor(c => c.Instructions.DistinctBy(i => i.Order).Count())
                    .Equal(c => c.Instructions.Count);
                RuleForEach(c => c.Ingredients)
                    .ChildRules(cr => {
                        cr.RuleFor(i => i.Name).NotEmpty();
                        cr.RuleFor(i => i.Quantity).GreaterThan(0);
                        cr.RuleFor(i => i.MeasurementUnit.ToEnum<MeasurementUnit>()).IsInEnum();
                        cr.RuleFor(i => i.LinkingProduct.Id).GreaterThan(0).When(i => i.LinkingProduct != null);
                        cr.RuleFor(i => i.LinkingProduct.SupermarketId).GreaterThan(0).When(i => i.LinkingProduct != null);
                        cr.RuleFor(i => i.LinkingProduct.Name).NotEmpty().When(i => i.LinkingProduct != null);
                    });
                RuleForEach(c => c.Instructions)
                    .ChildRules(cr =>
                    {
                        cr.RuleFor(i => i.Order).GreaterThanOrEqualTo(0);
                        cr.RuleFor(i => i.Description).NotEmpty();
                    });
            }
        }

        public class Handler : ICommandHandler<Command, Unit>
        {
            private readonly RecipeShopperContext _db;

            public Handler(RecipeShopperContext db)
            {
                _db = db;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var foundRecipe = _db.Recipes
                    .Where(r => r.Id == request.Id)
                    .Include(r => r.Ingredients).ThenInclude(i => i.Product).ThenInclude(p => p.Supermarket)
                    .Include(r => r.Instructions)
                    .FirstOrDefault();

                if (foundRecipe == null) throw new RecordNotFoundException($"Could not find the record with id ({request.Id}) to update");

                UpdateDbRecipe(request, foundRecipe);
                await _db.SaveChangesAsync();

                return Unit.Value;
            }

            private void UpdateDbRecipe(Command updatedRecipe, Recipe dbRecipe)
            {
                dbRecipe.Name = updatedRecipe.Name;
                dbRecipe.Description = updatedRecipe.Description;
                dbRecipe.Servings = updatedRecipe.Servings;
                dbRecipe.Tags = updatedRecipe.Tags;
                dbRecipe.DurationMinutes = updatedRecipe.DurationMinutes;

                var toUpdateIngredients = dbRecipe.Ingredients.Where(i => updatedRecipe.Ingredients.Any(updated => updated.Id == i.Id)).ToArray();
                var toUpdateInstructions = dbRecipe.Instructions.Where(i => updatedRecipe.Instructions.Any(updated => updated.Id == i.Id)).ToArray();

                /* Ingredients
                 * ************/
                // update
                foreach(var ingredient in toUpdateIngredients)
                {
                    var updatedIngredient = updatedRecipe.Ingredients.Single(i => i.Id == ingredient.Id);

                    ingredient.Name = updatedIngredient.Name;
                    ingredient.Unit = updatedIngredient.MeasurementUnit.ToEnum<MeasurementUnit>();
                    ingredient.Quantity = updatedIngredient.Quantity;
                    
                    if (updatedIngredient.LinkingProduct == null)
                    {
                        ingredient.Product = null;
                    } else
                    {
                        if (ingredient.Product == null)
                        {
                            ingredient.Product = new Product 
                            {
                                Id = updatedIngredient.LinkingProduct.Id,
                                Name = updatedIngredient.LinkingProduct.Name,
                                SupermarketId = updatedIngredient.LinkingProduct.SupermarketId
                            };
                        }
                        else
                        {
                            ingredient.Product.Id = updatedIngredient.LinkingProduct.Id;
                            ingredient.Product.Name = updatedIngredient.LinkingProduct.Name;
                            ingredient.Product.SupermarketId = updatedIngredient.LinkingProduct.SupermarketId;
                        }
                    }
                }

                // delete
                dbRecipe.Ingredients.RemoveAll(i => !toUpdateIngredients.Select(update => update.Id).Contains(i.Id));

                // create
                foreach(var toCreate in updatedRecipe.Ingredients.Where(i => !i.Id.HasValue))
                {
                    dbRecipe.Ingredients.Add(new Ingredient
                    {
                        Name = toCreate.Name,
                        Quantity = toCreate.Quantity,
                        Unit = toCreate.MeasurementUnit.ToEnum<MeasurementUnit>(),
                        Product = toCreate.LinkingProduct == null 
                            ? null 
                            : new Product
                            {
                                Id = toCreate.LinkingProduct.Id,
                                SupermarketId = toCreate.LinkingProduct.SupermarketId,
                                Name = toCreate.LinkingProduct.Name,
                            }
                    });
                }
                
                /* Instructions
                 * ************/
                // update
                foreach (var instruction in toUpdateInstructions)
                {
                    var updatedInstruction = updatedRecipe.Instructions.Single(i => i.Id == instruction.Id);

                    instruction.Order = updatedInstruction.Order;
                    instruction.Description = updatedInstruction.Description;
                }
                
                // delete
                dbRecipe.Instructions.RemoveAll(i => !toUpdateInstructions.Select(update => update.Id).Contains(i.Id));

                // create
                foreach (var toCreate in updatedRecipe.Instructions.Where(i => !i.Id.HasValue))
                {
                    dbRecipe.Instructions.Add(new Instruction
                    {
                        Order = toCreate.Order,
                        Description = toCreate.Description,
                    });
                }
            }
        }
    }
}
