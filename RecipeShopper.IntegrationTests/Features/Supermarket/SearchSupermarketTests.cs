using RecipeShopper.Application.Enums;
using RecipeShopper.Features.Supermarket;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace RecipeShopper.IntegrationTests.Features.Supermarket
{
    [Collection(nameof(VerticalSliceFixture))]
    public class SearchSupermarketTests
    {
        private readonly VerticalSliceFixture _fixture;
        private readonly SupermarketType[] _allSupermarketTypes = new SupermarketType[] { SupermarketType.Woolworths };

        public SearchSupermarketTests(VerticalSliceFixture fixture) => _fixture = fixture;
        [Fact]
        public async Task All_supermarkets_common_search_should_return_at_least_one_product()
        {
            var commonSearchTerm = "potato";

            var query = new SearchSupermarket.Query(commonSearchTerm, _allSupermarketTypes);
            var response = await _fixture.SendAsync(query);

            foreach (var products in response.ProductsBySupermarketDict.Values)
            {
                products.ShouldNotBeEmpty();
            }
        }

        // invalid_search_should_return_nothing
        [Fact]
        public async Task All_supermarkets_invalid_search_should_return_nothing()
        {
            var invalidSearchTerm = "h$!RFDC6#$@%@@%Fmknm45k2";

            var query = new SearchSupermarket.Query(invalidSearchTerm, _allSupermarketTypes);
            var response = await _fixture.SendAsync(query);

            foreach (var products in response.ProductsBySupermarketDict.Values)
            {
                products.ShouldBeEmpty();
            }
        }
    }
}
