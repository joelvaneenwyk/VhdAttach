namespace VhdAttach
{
    internal class PipeResponse
    {

        public PipeResponse(bool isError, string message)
        {
            IsError = isError;
            Message = message;
        }

        public bool IsError { get; private set; }
        public string Message { get; private set; }

    }
}
