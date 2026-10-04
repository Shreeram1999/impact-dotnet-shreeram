# Student API (Week 9)

The Week 8 API (see `Week_8/StudentApi/README.md`) with one addition for
Task 9.2: `ApiExceptionHandler` + `AddProblemDetails()`. Any exception no
controller or service turned into a deliberate status code becomes:

```
HTTP/1.1 500 Internal Server Error
Content-Type: application/problem+json

{ "type": "...", "title": "An unexpected error occurred.", "status": 500,
  "detail": "The request could not be completed. Please try again later.", "traceId": "00-..." }
```

The full exception is logged on the server with the same trace id. The body
never contains the exception message, a stack trace or connection details,
and `CanonicalServiceTests` asserts exactly that.

Status codes the React client handles (Task 9.2):

| Code | When | What the user sees |
|---|---|---|
| 400 | DataAnnotations / impossible DOB | the first server validation message, in the form |
| 401 | missing/expired/tampered token | logged out → login page with "Your session has expired" |
| 403 | Student calls a Teacher endpoint (even directly) | "You don't have permission to do that." |
| 404 | record deleted meanwhile | "That record no longer exists." |
| 409 | duplicate roll number / email / username | the server's message |
| 500 | anything unexpected | "The server hit a problem. Please try again." (GETs are retried once first) |

Databases: `StudentPortal_Week9` (scripts) and `StudentPortal_Week9_EfCodeFirst`
(migrations), set up as in Week 8.
