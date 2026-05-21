namespace HackerNews.Infrastructure.Exceptions
{
    public class ExternalServiceException : Exception
    {
        public int? StatusCode { get; }
        public string ErrorContent { get; }

        public ExternalServiceException(string message) : base(message)
        {
        }

        public ExternalServiceException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public ExternalServiceException(string message, int statusCode, string errorContent) : base(message)
        {
            StatusCode = statusCode;
            ErrorContent = errorContent;
        }

        public ExternalServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
