namespace RecipeShopper.Domain.Helpers
{
    public static class ResultHelper
    {
        public static Result Success(string message = null)
        {
            return new Result(true, message);
        }

        public static ResultData<T> SuccessWithData<T>(T data, string message = null)
        {
            return new ResultData<T>(true, data, message);
        }

        public static Result Fail(string message = null)
        {
            return new Result(false, message);
        }

        public static ResultData<T> FailWithData<T>(T data, string message = null)
        {
            return new ResultData<T>(false, data, message);
        }
    }
}
