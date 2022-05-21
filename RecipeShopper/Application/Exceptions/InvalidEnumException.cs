namespace RecipeShopper.Application.Exceptions
{
    public class InvalidEnumException : Exception
    {
        public InvalidEnumException() { }

        public InvalidEnumException(string message) : base(message) { }

        public InvalidEnumException(string message, Exception innerException) : base(message, innerException) { }
    }
}
