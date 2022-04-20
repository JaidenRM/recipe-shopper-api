using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecipeShopper.Application.Extensions;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Data;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Entities;
using RecipeShopper.Features.Supermarket;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace RecipeShopper.Features.Products
{
    public class GetProducts
    {
        //1. Query/Command - All the data we need to execute
        // Using record instead of class for ease, value-equality and immutability
        public record Query(Dictionary<int, int[]> IdsBySupermarketId) : IQuery<Response>;
        public record Model(int Id, string Name, decimal FullPrice, decimal CurrentPrice, int SupermarketId, string SupermarketName);

        public class MappingProfile : Profile 
        {
            public MappingProfile()
            {
                CreateMap<Product, Domain.Product>();
                CreateMap<Domain.Product, Model>();
            }
        }

        //2. Handler - Handle the data access stuff
        public class Handler : IQueryHandler<Query, Response>
        {
            private readonly RecipeShopperContext _db;
            private readonly IMapper _mapper;
            private readonly IMediator _mediator;

            public Handler(RecipeShopperContext dbContext, IConfigurationProvider config, IMapper mapper, IMediator mediator) {
                _db = dbContext;
                _mapper = mapper;
                _mediator = mediator;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                var products = _db.Products.ToList();

                if (request.IdsBySupermarketId != null && request.IdsBySupermarketId.Values.Any())
                    products = products
                        .Where(pro => request.IdsBySupermarketId.ContainsKey(pro.SupermarketId)
                            && request.IdsBySupermarketId.Values.Any(ids => ids.Any(id => id == pro.Id)))
                        .ToList();

                var domainResults = (await products.SelectAsync(MapFromSupermarketApi)).ToList();

                var results = _mapper
                    .Map<List<Domain.Product>, List<Model>>(domainResults)
                    .ToList();

                //3. Domain - If data is needed, convert to domain model which will handle all the behavioural stuff
                return new Response(results);
            }

            private async Task<Domain.Product> MapFromSupermarketApi(Product product)
            {
                var supermarketType = (SupermarketType)product.SupermarketId;

                var query = new SearchSupermarket.Query(product.Id.ToString(), new[] { supermarketType });
                
                var result = await _mediator.Send(query);
                var supermarketProduct = result.ProductsBySupermarketDict[supermarketType].First();

                var domainSupermarket = _mapper.Map<Domain.Supermarket>(product.Supermarket);
                return new Domain.Product(product.Id, supermarketProduct.Name, supermarketProduct.FullPrice, supermarketProduct.CurrentPrice, domainSupermarket);
            }
        }

        //4. Response - The data we want to return
        public record Response(List<Model> Results);
    }
}
