namespace StudentApi.Services;

// Task 5.4/5.5 - the non-generic result, used for Update/Delete: the
// controller only needs to know WHICH outcome happened to pick 204/404/409 -
// it never needs a payload back for those two verbs (Task 5.5 - PUT/DELETE
// return 204, not a body).
public class OperationResult
{
    public OperationOutcome Outcome { get; }
    public string Message { get; }

    protected OperationResult(OperationOutcome outcome, string message)
    {
        Outcome = outcome;
        Message = message;
    }

    public static OperationResult Ok(string message) => new(OperationOutcome.Success, message);

    public static OperationResult NotFound(string message) => new(OperationOutcome.NotFound, message);

    public static OperationResult Conflict(string message) => new(OperationOutcome.Conflict, message);
}

// Task 5.4/5.5 - the generic result, used for Add: POST needs the created
// entity back (its generated Id) to build the 201's Location header and
// response body, so this carries a Value on top of everything OperationResult
// already has.
public class OperationResult<T> : OperationResult
{
    public T? Value { get; }

    private OperationResult(OperationOutcome outcome, string message, T? value) : base(outcome, message)
    {
        Value = value;
    }

    public static OperationResult<T> Ok(T value, string message) => new(OperationOutcome.Success, message, value);

    public static new OperationResult<T> NotFound(string message) => new(OperationOutcome.NotFound, message, default);

    public static new OperationResult<T> Conflict(string message) => new(OperationOutcome.Conflict, message, default);
}
