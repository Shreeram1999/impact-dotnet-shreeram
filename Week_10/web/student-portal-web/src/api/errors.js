// Turns an axios error into one sentence a user can act on. Pages pass
// `overrides` when a status code means something specific in their
// context (e.g. 401 on the login form = wrong password, not "expired").
export function describeError(error, overrides = {}) {
  const status = error?.response?.status;

  if (status && overrides[status]) {
    return overrides[status];
  }

  if (!error?.response) {
    return "Can't reach the server. Is the API running?";
  }

  const data = error.response.data;
  switch (status) {
    case 400:
      return firstValidationMessage(data) ?? 'Please check the form and try again.';
    case 401:
      return 'Your session has expired. Please log in again.';
    case 403:
      return "You don't have permission to do that.";
    case 404:
      return 'That record no longer exists.';
    case 409:
      return typeof data === 'string' && data ? data : 'That conflicts with an existing record.';
    case 502:
    case 503:
    case 504:
      // Week 10 - the gateway is up but the service behind it isn't.
      return 'That part of the portal is temporarily unavailable. Please try again shortly.';
    default:
      return 'The server hit a problem. Please try again.';
  }
}

// ASP.NET Core's ValidationProblemDetails: { errors: { Field: ["message"] } }
function firstValidationMessage(data) {
  const errors = data?.errors;
  if (!errors) return null;
  const first = Object.values(errors)[0];
  return Array.isArray(first) && first.length ? first[0] : null;
}
