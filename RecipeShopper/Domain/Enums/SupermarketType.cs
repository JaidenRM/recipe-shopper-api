namespace RecipeShopper.Domain.Enums
{
    public enum SupermarketType
    {
        Woolworths = 1,
    }

    public static class SupermarketTypeExtensions
    {
        public static string ToFriendlyString(this SupermarketType type)
        {
            switch (type)
            {
                case SupermarketType.Woolworths:
                    return "Woolworths";
                default:
                    return null;
            }
        }
    }
}
