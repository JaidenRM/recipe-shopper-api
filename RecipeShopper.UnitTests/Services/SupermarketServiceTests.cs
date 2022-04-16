using Xunit;
using Shouldly;
using Moq;
using System.Threading.Tasks;
using RecipeShopper.Application.Interfaces;
using RecipeShopper.Domain;
using System.Collections.Generic;
using RecipeShopper.Infrastructure.Services;
using RecipeShopper.Application.Enums;

namespace RecipeShopper.UnitTests.Services
{
    public class SupermarketServiceTests
    {
        // 2. Mock test
        // null_response_should_return_empty_collection
        [Fact]
        public async Task Null_response_should_return_empty_collection()
        {
            var mockedSupermarketService = new Mock<ISupermarket>();
            
            // setup mocks
            mockedSupermarketService
                .Setup(u => u.Search(It.IsAny<string>()))
                .Returns(Task.FromResult<List<Product>>(null));
            mockedSupermarketService
                .Setup(u => u.GetSupermarketType())
                .Returns(SupermarketType.Woolworths);

            var supermarketService = new SupermarketService(new List<ISupermarket>() { mockedSupermarketService.Object });
            var result = await supermarketService.Search(SupermarketType.Woolworths, "haw haw");

            result.ShouldBeNull();
        }

        // empty_collection_response_should_return_empty_collection
        [Fact]
        public async Task Empty_collection_response_should_return_empty_collection()
        {
            var mockedSupermarketService = new Mock<ISupermarket>();

            // setup mocks
            mockedSupermarketService
                .Setup(u => u.Search(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<Product>()));
            mockedSupermarketService
                .Setup(u => u.GetSupermarketType())
                .Returns(SupermarketType.Woolworths);

            var supermarketService = new SupermarketService(new List<ISupermarket>() { mockedSupermarketService.Object });
            var result = await supermarketService.Search(SupermarketType.Woolworths, "haw haw");

            result.ShouldBeEmpty();
        }

        // single_product_should_return_collection_still
        [Fact]
        public async Task Single_product_should_return_collection_still()
        {
            var mockedSupermarketService = new Mock<ISupermarket>();
            var testProduct = new Product(1, "Naisuu", 42.69m, 33.33m);

            // setup mocks
            mockedSupermarketService
                .Setup(u => u.Search(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<Product>() { testProduct }));
            mockedSupermarketService
                .Setup(u => u.GetSupermarketType())
                .Returns(SupermarketType.Woolworths);

            var supermarketService = new SupermarketService(new List<ISupermarket>() { mockedSupermarketService.Object });
            var result = await supermarketService.Search(SupermarketType.Woolworths, "haw haw");

            result.Count.ShouldBe(1);
            result[0].ShouldBeEquivalentTo(testProduct);
        }

        // multiple_products_should_return_multiple
        [Fact]
        public async Task Multiple_products_should_return_multiple()
        {
            var mockedSupermarketService = new Mock<ISupermarket>();
            var testProducts = new List<Product>()
            {
                new Product(1, "Naisuu", 42.69m, 33.33m),
                new Product(145, "Watermaloooone", 4.99m, 3.33m),
                new Product(911, "Me", 5m, 5m),
            };

            // setup mocks
            mockedSupermarketService
                .Setup(u => u.Search(It.IsAny<string>()))
                .Returns(Task.FromResult(testProducts));
            mockedSupermarketService
                .Setup(u => u.GetSupermarketType())
                .Returns(SupermarketType.Woolworths);

            var supermarketService = new SupermarketService(new List<ISupermarket>() { mockedSupermarketService.Object });
            var results = await supermarketService.Search(SupermarketType.Woolworths, "haw haw");

            results.Count.ShouldBe(testProducts.Count);

            for (var i = 0; i < results.Count; i++)
            {
                results[i].ShouldBeEquivalentTo(testProducts[i]);
            }
        }
    }
}
