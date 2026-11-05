using System.Security.Claims;

namespace SmartPalm.CropService.Api.Controllers;

internal static class ClaimsPrincipalExtensions
{
    public static long RequiredUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.Sid)
            ?? principal.FindFirstValue("sid")
            ?? principal.FindFirstValue("sub");
        return long.TryParse(value, out var id) && id > 0
            ? id
            : throw new InvalidOperationException("The access token has no valid user identifier.");
    }

    public static string RequiredRole(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Role)
        ?? principal.FindFirstValue("role")
        ?? throw new InvalidOperationException("The access token has no role.");
}
