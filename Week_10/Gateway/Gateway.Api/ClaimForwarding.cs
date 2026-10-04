using StudentPortal.Shared;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Gateway.Api;

// Task 10.5 - "forward role claims". After the gateway has validated the
// JWT, it tells the service behind it who the caller is in plain headers:
//   X-User-Name, X-User-Role, X-User-Id
// Any copy of these headers sent by the CLIENT is removed first, on every
// route, so nobody can claim to be a Teacher just by adding a header.
//
// The original Authorization header is still forwarded, and Academics and
// Reporting re-validate the token themselves (defence in depth: the
// gateway is the front door, but a service that could ever be reached
// directly doesn't rely on it). The X-User-* headers give the services
// cheap identity for logging and auditing without parsing anything.
public static class ClaimForwarding
{
    public const string UserNameHeader = "X-User-Name";
    public const string RoleHeader = "X-User-Role";
    public const string UserIdHeader = "X-User-Id";

    public static void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transform =>
        {
            var headers = transform.ProxyRequest.Headers;
            headers.Remove(UserNameHeader);
            headers.Remove(RoleHeader);
            headers.Remove(UserIdHeader);

            var user = transform.HttpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                headers.TryAddWithoutValidation(UserNameHeader, user.FindFirst(PortalClaims.Name)?.Value);
                headers.TryAddWithoutValidation(RoleHeader, user.FindFirst(PortalClaims.Role)?.Value);
                headers.TryAddWithoutValidation(UserIdHeader, user.FindFirst(PortalClaims.UserId)?.Value);
            }

            return ValueTask.CompletedTask;
        });
    }
}
