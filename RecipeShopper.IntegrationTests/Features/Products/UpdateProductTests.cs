using Xunit;
using Shouldly;
using System.Threading.Tasks;
using RecipeShopper.Features.Products;
using RecipeShopper.Application.Exceptions;
using RecipeShopper.Domain.Enums;
using System.Collections.Generic;
using RecipeShopper.Entities;

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
            var product = new Product
            {
                Id = 378,
                SupermarketId = (int)SupermarketType.Woolworths,
                Name = "Pork Belly 500g"
            };
            await _fixture.InsertAsync(product);

            var updateCmd = new UpdateProduct.Command(product.Id, SupermarketType.Woolworths, "Pork Belly 1kg");
            await _fixture.SendAsync(updateCmd);

            var dbProd = await _fixture.FindAsync<Product>(product.Id, product.SupermarketId);
            dbProd.Name.ShouldBe(updateCmd.Name);
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
