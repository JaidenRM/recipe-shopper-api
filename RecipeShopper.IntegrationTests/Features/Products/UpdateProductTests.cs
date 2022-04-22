using Xunit;
using Shouldly;
using System.Threading.Tasks;
using RecipeShopper.Features.Products;
using RecipeShopper.Application.Exceptions;
using RecipeShopper.Domain.Enums;
using System.Collections.Generic;

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
            var createCmd = new CreateProduct.Command(378, SupermarketType.Woolworths, "Pork Belly 500g");
            var idBySupermarketId = new Dictionary<int, int[]>() { { (int)SupermarketType.Woolworths, new[] { createCmd.Id } } };

            await _fixture.SendAsync(createCmd);

            var getQuery = new GetProducts.Query(idBySupermarketId);
            var result = await _fixture.SendAsync(getQuery);

            result.Results.Count.ShouldBe(1);
            result.Results[0].Id.ShouldBe(createCmd.Id);

            var updateCmd = new UpdateProduct.Command(createCmd.Id, SupermarketType.Woolworths, "Pork Belly 1kg");
            await _fixture.SendAsync(updateCmd);

            var updatedQuery = await _fixture.SendAsync(getQuery);
            var updatedResult = updatedQuery.Results[0];

            result.Results.Count.ShouldBe(1);
            result.Results[0].ShouldBeEquivalentTo(updatedResult);

        }

        [Fact]
        public async Task Should_throw_if_product_does_not_exist()
        {
            var updateCmd = new UpdateProduct.Command(123, SupermarketType.Woolworths, "Pork Belly 1kg");
            var idBySupermarketId = new Dictionary<int, int[]>() { { (int)SupermarketType.Woolworths, new[] { updateCmd.Id } } };

            var getQuery = new GetProducts.Query(idBySupermarketId);
            var result = await _fixture.SendAsync(getQuery);

            result.Results.ShouldBeEmpty();

            var updateFn = async () => await _fixture.SendAsync(updateCmd);

            updateFn.ShouldThrow<RecordNotFoundException>();
        }
    }
}
