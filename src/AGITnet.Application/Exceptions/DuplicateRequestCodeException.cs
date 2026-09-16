namespace AGITnet.Application.Exceptions;

public class DuplicateRequestCodeException : Exception
{
    public string RequestCode { get; }

    public DuplicateRequestCodeException(string requestCode)
        : base($"RequestCode '{requestCode}' sudah ada dalam sistem.")
    {
        RequestCode = requestCode;
    }
}
