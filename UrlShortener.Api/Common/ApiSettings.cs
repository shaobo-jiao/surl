namespace UrlShortener.Api;

public class ApiSettings
{
    /// <summary>
    /// Hosts disallowed to shorten urls, including the app domains themselves 
    /// </summary>
    public string[] DisallowedHosts { get; set; } = [];
}
