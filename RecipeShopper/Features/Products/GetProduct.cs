using MediatR;
using RecipeShopper.Data;
using RecipeShopper.Entities;

namespace RecipeShopper.Features.Products
{
    public static class GetProduct
    {
        //1. Query/Command - All the data we need to execute
        // Using record instead of class for ease, value-equality and immutability
        public record Query(int? Id) : IRequest<Response>;

        //2. Handler - Handle the data access stuff
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly RecipeShopperContext _db;
            // Inject DB
            public Handler(RecipeShopperContext dbContext) {
                _db = dbContext;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                var products = _db.Products.ToArray();
                //var resp = new Response { Results = products };
                //3. Domain - If data is needed, convert to domain model which will handle all the behavioural stuff
                return new Response(products);
            }
        }

        //4. Response - The data we want to return
        public record Response(Product[] Results);
    }
}
