namespace Services.Exceptions
{
    public class FileProcessorException : Exception
    {
        public string ErrorCode { get; }

        public FileProcessorException(string errorCode)
            : base(errorCode)
        {
            ErrorCode = errorCode;
        }

        public FileProcessorException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public FileProcessorException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
