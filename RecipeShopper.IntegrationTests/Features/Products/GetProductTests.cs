using System.Threading.Tasks;
using RecipeShopper.Entities;
using RecipeShopper.Features.Products;
using Xunit;
using Shouldly;
using System.Linq;

namespace RecipeShopper.IntegrationTests.Features.Products
{
    [Collection(nameof(VerticalSliceFixture))]
    public class GetProductTests
    {
        private readonly VerticalSliceFixture _fixture;

        public GetProductTests(VerticalSliceFixture verticalSliceFixture) => _fixture = verticalSliceFixture;

        [Fact]
        public async Task Get_all_products_by_default()
        {
            var prod1 = new Product
            {
                Id = 5,
                Name = "Product5",
                CurrentPrice = 14.95M,
                FullPrice = 14.95M,
            };

            var prod2 = new Product
            {
                Id = 7,
                Name = "Product8",
                CurrentPrice = 2.88M,
                FullPrice = 7.5M,
            };

            await _fixture.InsertAsync(prod1, prod2);

            var query = new GetProduct.Query(null);
            var result = await _fixture.SendAsync(query);

            result.Results.Length.ShouldBe(2);
            result.Results.Select(r => r.Id).ShouldContain(prod1.Id);
            result.Results.Select(r => r.Id).ShouldContain(prod2.Id);
        }
    }
}
