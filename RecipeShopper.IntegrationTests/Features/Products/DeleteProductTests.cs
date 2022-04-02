using RecipeShopper.Entities;
using RecipeShopper.Features.Products;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace RecipeShopper.IntegrationTests.Features.Products
{
    [Collection(nameof(VerticalSliceFixture))]
    public class DeleteProductTests
    {
        private readonly VerticalSliceFixture _fixture;

        public DeleteProductTests(VerticalSliceFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Should_delete_product()
        {
            var createCmd = new CreateProduct.Command(1, "Whittaker's Chocolate", 5M, 5M);
            await _fixture.SendAsync(createCmd);

            var getQuery = new GetProducts.Query(new[] { createCmd.Id });
            var result = await _fixture.SendAsync(getQuery);

            result.Results.Count.ShouldBe(1);
            result.Results[0].Id.ShouldBe(createCmd.Id);

            var delCmd = new DeleteProduct.Command(createCmd.Id);

            await _fixture.SendAsync(delCmd);

            var deletedResult = await _fixture.SendAsync(getQuery);

            deletedResult.Results.ShouldBeEmpty();
        }

        [Fact]
        public async Task Should_complete_deleting_nothing()
        {
            var testId = 1;
            var getQuery = new GetProducts.Query(new[] { testId });
            var result = await _fixture.SendAsync(getQuery);

            result.Results.ShouldBeEmpty();

            var delCmd = new DeleteProduct.Command(testId);

            await _fixture.SendAsync(delCmd);
        }
    }
}
