using Xunit;
using Shouldly;
using System.Threading.Tasks;
using RecipeShopper.Features.Products;
using RecipeShopper.Application.Exceptions;

namespace RecipeShopper.IntegrationTests.Features.Products
{
    [Collection(nameof(VerticalSliceFixture))]
    public class UpdateProductTests
    {
        private readonly VerticalSliceFixture _fixture;

        public UpdateProductTests(VerticalSliceFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Should_update_product()
        {
            var createCmd = new CreateProduct.Command(378, "Pork Belly 500g", 11M, 10M);
            await _fixture.SendAsync(createCmd);

            var getQuery = new GetProducts.Query(new[] { createCmd.Id });
            var result = await _fixture.SendAsync(getQuery);

            result.Results.Count.ShouldBe(1);
            result.Results[0].Id.ShouldBe(createCmd.Id);

            var updateCmd = new UpdateProduct.Command(createCmd.Id, "Pork Belly 1kg", 22M, 20M);
            await _fixture.SendAsync(updateCmd);

            var updatedQuery = await _fixture.SendAsync(getQuery);
            var updatedResult = updatedQuery.Results[0];

            result.Results.Count.ShouldBe(1);
            result.Results[0].ShouldBeEquivalentTo(updatedResult);

        }

        [Fact]
        public async Task Should_throw_if_product_does_not_exist()
        {
            var updateCmd = new UpdateProduct.Command(123, "Pork Belly 1kg", 22M, 20M);
            
            var getQuery = new GetProducts.Query(new[] { updateCmd.Id });
            var result = await _fixture.SendAsync(getQuery);

            result.Results.ShouldBeEmpty();

            var updateFn = async () => await _fixture.SendAsync(updateCmd);

            updateFn.ShouldThrow<RecordNotFoundException>();
        }
    }
}
