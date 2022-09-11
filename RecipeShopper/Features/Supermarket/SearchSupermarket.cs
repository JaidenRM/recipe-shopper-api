using AutoMapper;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Domain;
using RecipeShopper.Infrastructure.Services;

namespace RecipeShopper.Features.Supermarket
{
    public class SearchSupermarket
    {
        public record Query(string SearchTerm, SupermarketType[] Supermarkets) : IQuery<Response>;

        /// <summary>Represents a product from a supermarket</summary>
        public record Model(int Id, string Name, decimal FullPrice, decimal CurrentPrice, ImageSet ImageUrls)
        {
            /// <summary>Represents the id of this product from a specific store</summary>
            /// <example>34578</example>
            public int Id { get; init; } = Id;
            /// <summary>Name of the product from the store</summary>
            /// <example>Cadbury Milk Chocolate Block (180g)</example>
            public string Name { get; init; } = Name;
            /// <summary>The normal RRP of the item from this store</summary>
            /// <example>3.99</example>
            public decimal FullPrice { get; init; } = FullPrice;
            /// <summary>The price of the item from the store as of now. Useful for indicating if it is on sale</summary>
            /// <example>4.99</example>
            public decimal CurrentPrice { get; init; } = CurrentPrice;
            /// <summary>Varying sizes of the same image at different urls</summary>
            /// <example>{ Small: "cdn.example.com/img/ball1_small.png", Medium: null, Large: "cdn.example.com/img/ball1_large.png" }</example>
            public ImageSet ImageUrls { get; init; } = ImageUrls;
        }
        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<Product, Model>();
        }

        public class Handler : IQueryHandler<Query, Response>
        {
            private readonly SupermarketService _supermarketService;
            private readonly IMapper _mapper;

            public Handler(SupermarketService supermarketService, IMapper mapper)
            {
                _supermarketService = supermarketService;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                var productsBySupermarket = new Dictionary<SupermarketType, List<Model>>();
                var results = await _supermarketService.SearchSome(request.Supermarkets, request.SearchTerm);

                foreach(var (supermarket, products) in results)
                {
                    productsBySupermarket[supermarket] = _mapper.Map<List<Model>>(products);
                }

                return new Response(productsBySupermarket);
            }
        } 

        public record Response(Dictionary<SupermarketType, List<Model>> ProductsBySupermarketDict);
    }
}
