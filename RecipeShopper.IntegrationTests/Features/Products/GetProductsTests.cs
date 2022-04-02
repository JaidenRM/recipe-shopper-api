using System.Threading.Tasks;
using RecipeShopper.Entities;
using RecipeShopper.Features.Products;
using Xunit;
using Shouldly;
using System.Linq;

namespace RecipeShopper.IntegrationTests.Features.Products
{
    [Collection(nameof(VerticalSliceFixture))]
    public class GetProductsTests
    {
        private readonly VerticalSliceFixture _fixture;

        public GetProductsTests(VerticalSliceFixture verticalSliceFixture) => _fixture = verticalSliceFixture;

        [Fact]
        public async Task Get_all_products_by_default()
        {
            var prod1 = new Product
            {
                Id = 555,
                Name = "Lean Mince 800g",
                CurrentPrice = 14.95M,
                FullPrice = 14.95M,
            };

            var prod2 = new Product
            {
                Id = 777,
                Name = "Tim Tam's",
                CurrentPrice = 2.99M,
                FullPrice = 3.99M,
            };

            await _fixture.InsertAsync(prod1, prod2);

            var query = new GetProducts.Query(null);
            var result = await _fixture.SendAsync(query);

            result.Results.Count.ShouldBe(2);
            result.Results.Select(r => r.Id).ShouldContain(prod1.Id);
            result.Results.Select(r => r.Id).ShouldContain(prod2.Id);

            await _fixture.ResetCheckpoint();
        }

        [Fact]
        public async Task Get_specific_products()
        {
            var prod1 = new Product
            {
                Id = 321,
                Name = "Muesli",
                CurrentPrice = 4.25M,
                FullPrice = 5.20M,
            };

            var prod2 = new Product
            {
                Id = 256,
                Name = "Bag of potatoes",
                CurrentPrice = 6.6M,
                FullPrice = 6.6M,
            };

            var prod3 = new Product
            {
                Id = 839,
                Name = "Bread",
                CurrentPrice = 2.95M,
                FullPrice = 3.65M,
            };

            await _fixture.InsertAsync(prod1, prod2, prod3);

            var query = new GetProducts.Query(new[] { prod2.Id, prod3.Id });
            var result = await _fixture.SendAsync(query);

            result.Results.Count.ShouldBe(2);
            result.Results.Select(r => r.Id).ShouldContain(prod2.Id);
            result.Results.Select(r => r.Id).ShouldContain(prod3.Id);

            await _fixture.ResetCheckpoint();
        }
    }
}
