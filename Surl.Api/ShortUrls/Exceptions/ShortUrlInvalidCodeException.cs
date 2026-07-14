namespace Surl.Api.ShortUrls;

public class ShortUrlInvalidCodeException: ShortUrlBaseException
{
    public ShortUrlInvalidCodeException(): base() {}
    public ShortUrlInvalidCodeException(string message): base(message) {}
}
