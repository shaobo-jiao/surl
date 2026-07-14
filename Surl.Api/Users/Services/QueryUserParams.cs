namespace Surl.Api.Users;

public record QueryUserParams(string? Username, string? Email, string? OrderBy, bool? IsDescending, int? Page, int? PageSize);
