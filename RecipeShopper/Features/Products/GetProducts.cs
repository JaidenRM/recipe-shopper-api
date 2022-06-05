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
        public record Query(Dictionary<int, int[]> IdsBySupermarketId) : IQuery<Response>;
        public record Model(int Id, string Name, decimal FullPrice, decimal CurrentPrice, int SupermarketId, string SupermarketName);

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
