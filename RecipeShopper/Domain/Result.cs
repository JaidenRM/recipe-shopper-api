namespace RecipeShopper.Domain
{
    public class Result
    {
        public bool IsSuccessful { get; private set; }
        public string Message { get; private set; }

        public Result(bool isSuccessful, string message)
        {
            IsSuccessful = isSuccessful; 
            Message = message;
        }
    }

    public class ResultData<T> : Result
    {
        public T Data { get; private set; }

        public ResultData(bool isSuccessful, T data, string message) : base(isSuccessful, message)
        { 
            Data = data;
        }
    }
}
