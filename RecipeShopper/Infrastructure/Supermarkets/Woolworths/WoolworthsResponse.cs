namespace RecipeShopper.Infrastructure.Supermarkets.Woolworths
{
    public class WoolworthsResponse
    {
        public WoolworthsProductHolder[] Products { get; set; }
    }

    public class WoolworthsProductHolder
    {
        public WoolworthsProduct[] Products { get; set; }
    }
}
