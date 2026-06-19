namespace Surl.Api;

public class ShortUrlInvalidCodeException: ShortUrlBaseException
{
    public ShortUrlInvalidCodeException(): base() {}
    public ShortUrlInvalidCodeException(string message): base(message) {}
}
