namespace Surl.Api;

public class ShortUrlExpiredException : ShortUrlBaseException
{
    public ShortUrlExpiredException() : base() { }
    public ShortUrlExpiredException(string message) : base(message) { }
}
