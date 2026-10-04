# Day 4 notes: the applied Strategy/Factory pattern (Task 5.8)

## What's swappable, and where

`GET api/students/{id}/grade?scale=percentage|gpa` is backed by:

- `IGradeStrategy` - one method, `Describe(int score)` - same shape as Week
  3's `IPaymentStrategy` (`Day_3_Task_3.7`).
- `PercentageGradeStrategy` / `GpaGradeStrategy` - the two concrete
  algorithms.
- `IGradeStrategyFactory` / `GradeStrategyFactory` - picks which one, from
  the `?scale=` query string, case-insensitively, falling back to
  Percentage for anything unrecognized (so there's no invalid `scale` from
  the caller's point of view).

`StudentsController.GetGrade` only ever calls
`gradeStrategyFactory.Create(scale)` and then `strategy.Describe(score)` -
it has no `if (scale == "gpa")` anywhere. Proof this is genuinely swappable
without touching the controller: `GradeStrategyFactoryTests.cs` constructs
`GradeStrategyFactory` directly and asserts `Create("gpa")` returns a
`GpaGradeStrategy` and everything else returns `PercentageGradeStrategy` -
none of those tests touch `StudentsController` at all, and neither would a
hypothetical third strategy (say, `LetterGradeStrategy`) - it would only
mean adding one more case to the factory's switch expression.

## Why Singleton in Program.cs

```csharp
builder.Services.AddSingleton<IGradeStrategyFactory, GradeStrategyFactory>();
```

The factory carries no state of its own - `Create(scale)` is a pure
function of its input - so there's nothing request-specific about it that
would need a fresh instance per request (Scoped) or per resolution
(Transient). One shared instance for the app's lifetime is both correct and
slightly cheaper than re-allocating a factory object on every single grade
request.

## Why this beat the response-formatter-factory alternative

The task offered two options: an `IGradeStrategy` (Percentage vs GPA) or a
response-formatter factory (e.g. picking JSON vs a plain-text shape per
request). Grade scaling was chosen because it's a genuine BUSINESS decision
- which number a school actually wants to see - rather than a wire-format
concern, which keeps the pattern living in `/Services` where business rules
belong, instead of blurring into presentation logic that arguably belongs
closer to the HTTP layer.
