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
        public record Command(int Id, string Name, string Description, int Servings, int DurationMinutes, string Tags, List<UpdateIngredient> Ingredients, List<UpdateInstruction> Instructions) : ICommand<Unit>;
        public record UpdateProduct(int Id, int SupermarketId, string Name);
        public record UpdateIngredient(int? Id, string Name, decimal Quantity, string MeasurementUnit, UpdateProduct LinkingProduct);
        public record UpdateInstruction(int? Id, int Order, string Description);

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
