using FluentValidation;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Features.Products;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace RecipeShopper.UnitTests.Features.Products
{
    public class GetProductsTests
    {
        private AbstractValidator<GetProducts.Query> _validator;

        public GetProductsTests()
        {
            _validator = new GetProducts.Validator();
        }

        [Fact]
        public void Pass_on_valid_supermarket_empty_product_value()
        {
            var query = new GetProducts.Query(new Dictionary<int, int[]>
            {
                { (int) SupermarketType.Woolworths, new int[] { } }
            });

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Pass_on_valid_supermarket_single_product_value()
        {
            var query = new GetProducts.Query(new Dictionary<int, int[]>
            {
                { (int) SupermarketType.Woolworths, new int[] { 123 } }
            });

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Pass_on_valid_supermarket_multiple_product_values()
        {
            var query = new GetProducts.Query(new Dictionary<int, int[]>
            {
                { (int) SupermarketType.Woolworths, new int[] { 123, 456, 789 } }
            });

            _validator.Validate(query).IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Fail_on_empty_dictionary()
        {
            var query = new GetProducts.Query(new Dictionary<int, int[]>());

            _validator.Validate(query).IsValid.ShouldBeFalse();
        }

        [Fact]
        public void Fail_on_empty_null()
        {
            var query = new GetProducts.Query(null);

            _validator.Validate(query).IsValid.ShouldBeFalse();
        }

        [Fact]
        public void Fail_on_invalid_supermarket_key()
        {
            var query = new GetProducts.Query(new Dictionary<int, int[]>
            {
                { 124345356, new int[] { } }
            });

            _validator.Validate(query).IsValid.ShouldBeFalse();
        }

        [Fact]
        public void Fail_on_invalid_and_valid_supermarket_key()
        {
            var query = new GetProducts.Query(new Dictionary<int, int[]>
            {
                { 124345356, new int[] { } },
                { (int) SupermarketType.Woolworths, new int[] { } }
            });

            _validator.Validate(query).IsValid.ShouldBeFalse();
        }

        [Fact]
        public void Fail_on_null_product_value()
        {
            var query = new GetProducts.Query(new Dictionary<int, int[]>
            {
                { (int) SupermarketType.Woolworths, null }
            });

            //query.IdsBySupermarketId.Values.ShouldAllBe(v => v != null);
            _validator.Validate(query).IsValid.ShouldBeFalse();
        }
    }
}
