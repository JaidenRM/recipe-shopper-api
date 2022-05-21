namespace RecipeShopper.Application.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsEqualWithin(this DateTime dt1, DateTime dt2, TimeSpan within)
        {
            return (dt1 - dt2) <= within;
        }
    }
}
