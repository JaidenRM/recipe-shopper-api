using RecipeShopper.Domain.Enums;
using RecipeShopper.Features.Products;
using Shouldly;
using System.Collections.Generic;
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
            var createCmd = new CreateProduct.Command(1, SupermarketType.Woolworths, "Whittaker's Chocolate");
            await _fixture.SendAsync(createCmd);

            var idBySupermarketId = new Dictionary<int, int[]>() { { (int)SupermarketType.Woolworths, new[] { createCmd.Id } } };
            var getQuery = new GetProducts.Query(idBySupermarketId);
            var result = await _fixture.SendAsync(getQuery);

            result.Results.Count.ShouldBe(1);
            result.Results[0].Id.ShouldBe(createCmd.Id);

            var delCmd = new DeleteProduct.Command(createCmd.Id, createCmd.SupermarketType);

            await _fixture.SendAsync(delCmd);

            var deletedResult = await _fixture.SendAsync(getQuery);

            deletedResult.Results.ShouldBeEmpty();
        }

        [Fact]
        public async Task Should_complete_deleting_nothing()
        {
            var testId = 1;
            var idBySupermarketId = new Dictionary<int, int[]>() { { (int)SupermarketType.Woolworths, new[] { testId } } };

            var getQuery = new GetProducts.Query(idBySupermarketId);
            var result = await _fixture.SendAsync(getQuery);

            result.Results.ShouldBeEmpty();

            var delCmd = new DeleteProduct.Command(testId, SupermarketType.Woolworths);

            await _fixture.SendAsync(delCmd);
        }
    }
}
