using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Api;

public class ApiSettings
{
    [Required(AllowEmptyStrings = false)]
    public string ApiBaseUrl { get; set; } = string.Empty;
}
