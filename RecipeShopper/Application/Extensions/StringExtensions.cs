using RecipeShopper.Domain.Enums;

namespace RecipeShopper.Application.Extensions
{
    public static class StringExtensions
    {
        public static T ToEnum<T>(this string enumStr)
        {
            var requestedType = typeof(T);
            if (!requestedType.IsEnum) throw new InvalidCastException($"Type ({requestedType}) is not an enum. Only enum types are allowed for this extension.");

            if (requestedType == typeof(MeasurementUnit))
                return (T)Convert.ChangeType(MeasurementUnitExtensions.FromString(enumStr), requestedType);

            throw new InvalidCastException($"Enum type ({requestedType}) has not been implemented for this extension.");
        }
    }
}
