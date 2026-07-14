namespace Surl.Api.ShortUrls;

public class ShortUrlBaseException: Exception
{
    public ShortUrlBaseException() : base() { }
    public ShortUrlBaseException(string message) : base(message){}
}
