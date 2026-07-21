namespace Surl.Api.Users;

/// <summary>
/// Parameters that can be used when querying users.
/// </summary>
/// <param name="Username">filter by username</param>
/// <param name="Email">filter by email</param>
/// <param name="OrderBy">the property to order the final user list.</param>
/// <param name="IsDescending">works with OrderBy to define whether the list is ascending or descending.</param>
/// <param name="Page">works with PageSize, to specify the page number.</param>
/// <param name="PageSize">works with Page, to define the page size.</param>
public record QueryUserParams(string? Username, string? Email, string? OrderBy, bool? IsDescending, int? Page, int? PageSize);
