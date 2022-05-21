using RecipeShopper.Application.Exceptions;

namespace RecipeShopper.Domain.Enums
{
    public enum MeasurementUnit
    {
        None,
        Each,
        Teaspoon,
        Tablespoon,
        Grams,
        Kilograms,
        Millilitres,
        Litres,
        Cup,
        Pinch
    }
    public static class MeasurementUnitExtensions
    {
        public static string ToFriendlyString(this MeasurementUnit unit)
        {
            switch (unit)
            {
                case MeasurementUnit.None:
                    return "none";
                case MeasurementUnit.Each:
                    return "each";
                case MeasurementUnit.Teaspoon:
                    return "teaspoon";
                case MeasurementUnit.Tablespoon:
                    return "tablespoon";
                case MeasurementUnit.Grams:
                    return "grams";
                case MeasurementUnit.Kilograms:
                    return "kilograms";
                case MeasurementUnit.Millilitres:
                    return "millilitres";
                case MeasurementUnit.Litres:
                    return "litres";
                case MeasurementUnit.Cup:
                    return "cup";
                case MeasurementUnit.Pinch:
                    return "pinch";
                default:
                    throw new InvalidEnumException("(" + unit + ") in the " + nameof(MeasurementUnit) + " enum isn't implemented in this extension method");
            }
        }

        public static MeasurementUnit FromString(string unitName)
        {
            switch(unitName.ToLower())
            {
                case "none":
                    return MeasurementUnit.None;
                case "each":
                    return MeasurementUnit.Each;
                case "teaspoon":
                    return MeasurementUnit.Teaspoon;
                case "tablespoon":
                    return MeasurementUnit.Tablespoon;
                case "grams":
                    return MeasurementUnit.Grams;
                case "kilograms":
                    return MeasurementUnit.Kilograms;
                case "millilitres":
                    return MeasurementUnit.Millilitres;
                case "litres":
                    return MeasurementUnit.Litres;
                case "cup":
                    return MeasurementUnit.Cup;
                case "pinch":
                    return MeasurementUnit.Pinch;
                default:
                    throw new InvalidEnumException("Could not convert (" + unitName + ") into a valid " + nameof(MeasurementUnit) + " enum");
            }
        }
    }
}
