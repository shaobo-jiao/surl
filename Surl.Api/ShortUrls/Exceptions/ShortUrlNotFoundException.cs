namespace Surl.Api.ShortUrls;

public class ShortUrlNotFoundException : ShortUrlBaseException
{
    public ShortUrlNotFoundException() : base() { }
    public ShortUrlNotFoundException(string message) : base(message) { }
}
