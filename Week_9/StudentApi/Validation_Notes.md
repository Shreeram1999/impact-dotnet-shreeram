# Day 3 notes: what [ApiController] is actually doing (Task 5.6)

## With [ApiController] present (the shipped state)

`StudentCreateDto`'s attributes - `[Required]`, `[Range(5, 100)]`,
`[EmailAddress]` - are checked automatically, before
`StudentsController.Create`/`Update` ever run. POSTing an out-of-range age
never reaches the action body at all:

```
POST /api/students   { "name": "Too Old", "age": 200, "rollNumber": "R3", ... }
-> 400 Bad Request, application/problem+json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Age": ["The field Age must be between 5 and 100."] }
}
```

No line in `Create` checks `ModelState` - `[ApiController]` registers a
model-validation filter that runs before the action, and short-circuits with
that ProblemDetails response if `ModelState.IsValid` is false.

## With [ApiController] temporarily removed (actually tried, not just read about)

Commenting out `[ApiController]` on `StudentsController` and re-running the
exact same request produced something more surprising than "the bad age
gets through":

```
POST /api/students   { "name": "Too Old", "age": 200, "rollNumber": "NOAPICTRL1", "email": "old@example.com", "score": 50 }
-> 201 Created
{ "id": 1, "name": "", "age": 0, "rollNumber": "", "email": "", "score": 0 }
```

Every field came back at its DEFAULT, not the values that were actually
sent. `[ApiController]` is also what makes a complex parameter like
`StudentCreateDto` get inferred as `[FromBody]` automatically; without it,
ASP.NET Core falls back to binding from route values and the query string,
finds no matching `dto.*` values there, and hands the action a completely
empty `StudentCreateDto` - which then sails through untouched (there's no
validation running either) and gets stored as-is. So the request didn't
"succeed with bad data" - it silently stored the WRONG data entirely, with
no error anywhere to notice by.

A plain GET (`GET /api/students/1`, no body to bind) was unaffected either
way - this only shows up on the POST/PUT actions that take a body.

Restoring `[ApiController]` - with no other code change - brought both the
automatic 400 and correct body binding back immediately.

## Takeaway

`[ApiController]` isn't decorative - it's doing two jobs at once: turning
`StudentCreateDto`'s attributes into an automatic 400, and inferring
`[FromBody]` so the JSON body actually gets bound to the parameter at all.
Losing the second one is the more dangerous failure mode: it doesn't throw
or reject anything, it just quietly stores empty data with a 201 that looks
like success.
