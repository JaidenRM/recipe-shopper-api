using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecipeShopper.Application.Extensions;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Entities;
using RecipeShopper.Features.Supermarket;

namespace RecipeShopper.Features.Products
{
    public class GetProducts
    {
        //1. Query/Command - All the data we need to execute
        // Using record instead of class for ease, value-equality and immutability
        /// <summary>Query for a dictionary of products from supermarket(s). Will return products from each supermarket based on the productIds passed.</summary>
        /// <param name="IdsBySupermarketId">SupermarketId is used as a key and an array of productIds for values</param>
        public record Query(Dictionary<int, int[]> IdsBySupermarketId) : IQuery<Response>;

        /// <summary>Represents a product from a specific supermarket in regards to an ingredient in a recipe</summary>
        public record Model(int Id, string Name, decimal FullPrice, decimal CurrentPrice, int SupermarketId, string SupermarketName)
        {
            /// <summary>Represents the id of this product from a specific store</summary>
            /// <example>123</example>
            public int Id { get; init; } = Id;
            /// <summary>Name of the product from the store</summary>
            /// <example>Bob's All-Natural Tomato Paste</example>
            public string Name { get; init; } = Name;
            /// <summary>The normal RRP of the item from this store</summary>
            /// <example>1.99</example>
            public decimal FullPrice { get; init; } = FullPrice;
            /// <summary>The price of the item from the store as of now. Useful for indicating if it is on sale</summary>
            /// <example>1.49</example>
            public decimal CurrentPrice { get; init; } = CurrentPrice;
            /// <summary>The internal id used to track which supermarket this product is from</summary>
            /// <example>1</example>
            public int SupermarketId { get; init; } = SupermarketId;
            /// <example>Bob's Grocer</example>
            public string SupermarketName { get; init; } = SupermarketName;
        };

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(q => q.IdsBySupermarketId).NotEmpty();
                RuleFor(q => q.IdsBySupermarketId.Keys.All(sId => Enum.IsDefined(typeof(SupermarketType), sId))).Equal(true).When(q => q.IdsBySupermarketId != null);
                RuleFor(q => q.IdsBySupermarketId.Values.All(pIds => pIds != null)).Equal(true).When(q => q.IdsBySupermarketId != null);
            }
        }

        //2. Handler - Handle the data access stuff
        public class Handler : IQueryHandler<Query, Response>
        {
            private readonly RecipeShopperContext _db;
            private readonly IMediator _mediator;

            public Handler(RecipeShopperContext dbContext, IMediator mediator) {
                _db = dbContext;
                _mediator = mediator;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                IQueryable<Product> products = _db.Products.Include(p => p.Supermarket);

                if (request.IdsBySupermarketId != null && request.IdsBySupermarketId.Values.Any())
                    products = products
                        .AsEnumerable()
                        .Where(pro => request.IdsBySupermarketId.ContainsKey(pro.SupermarketId)
                            && request.IdsBySupermarketId.Values.Any(ids => ids.Any(id => id == pro.Id)))
                        .AsQueryable();

                var mappedResults = (await products.SelectAsync(MapFromSupermarketApi)).ToList();

                return new Response(mappedResults);
            }

            private async Task<Model> MapFromSupermarketApi(Product product)
            {
                var supermarketType = (SupermarketType)product.SupermarketId;

                var query = new SearchSupermarket.Query(product.Id.ToString(), new[] { supermarketType });
                
                var result = await _mediator.Send(query);
                var supermarketProduct = result.ProductsBySupermarketDict[supermarketType].FirstOrDefault();

                return new Model(product.Id, product.Name, supermarketProduct?.FullPrice ?? 0, supermarketProduct?.CurrentPrice ?? 0, product.Supermarket.Id, product.Supermarket.Name);
            }
        }

        //4. Response - The data we want to return
        public record Response(List<Model> Results);
    }
}
