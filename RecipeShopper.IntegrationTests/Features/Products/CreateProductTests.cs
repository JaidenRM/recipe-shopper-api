using RecipeShopper.Entities;
using RecipeShopper.Features.Products;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using FluentValidation;
using RecipeShopper.Domain.Enums;

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
            var cmd = new CreateProduct.Command(458, SupermarketType.Woolworths, "Gummy Bears");

            await _fixture.SendAsync(cmd);

            var prod = await _fixture.FindAsync<Product>(cmd.Id, (int)cmd.SupermarketType);

            prod.Id.ShouldBeEquivalentTo(cmd.Id);
        }

        public async Task Should_not_create_empty_product()
        {
            var cmd = new CreateProduct.Command(0, 0, "");
            var sendCmd = async () => await _fixture.SendAsync(cmd);

            await sendCmd.ShouldThrowAsync<ValidationException>();

            var prod = await _fixture.FindAsync<Product>(cmd.Id, (int)cmd.SupermarketType);

            prod.ShouldBeNull();
        }
    }
}
