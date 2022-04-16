using RecipeShopper.Entities;
using RecipeShopper.Features.Products;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using FluentValidation;

namespace RecipeShopper.IntegrationTests.Features.Products
{
    [Collection(nameof(VerticalSliceFixture))]
    public class CreateProductTests
    {
        private readonly VerticalSliceFixture _fixture;

        public CreateProductTests(VerticalSliceFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task Should_create_product()
        {
            var cmd = new CreateProduct.Command(458, "Gummy Bears", 2.00M, 2.00M);

            await _fixture.SendAsync(cmd);

            var prod = await _fixture.FindAsync<Product>(cmd.Id);

            prod.Id.ShouldBeEquivalentTo(cmd.Id);
        }

        [Fact]
        public async Task Should_not_create_product_with_current_price_higher_than_full_price()
        {
            var cmd = new CreateProduct.Command(123, "Apples", 2.00M, 4.00M);
            var sendCmd = async () => await _fixture.SendAsync(cmd);

            await sendCmd.ShouldThrowAsync<ValidationException>();

            var prod = await _fixture.FindAsync<Product>(cmd.Id);

            prod.ShouldBeNull();
        }

        public async Task Should_not_create_empty_product()
        {
            var cmd = new CreateProduct.Command(0, "", 0, 0);
            var sendCmd = async () => await _fixture.SendAsync(cmd);

            await sendCmd.ShouldThrowAsync<ValidationException>();

            var prod = await _fixture.FindAsync<Product>(cmd.Id);

            prod.ShouldBeNull();
        }
    }
}
