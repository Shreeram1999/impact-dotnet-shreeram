namespace StudentManagementApp.Services;

// Task 4.4 - the success/failure signal every service method returns.
//
// Why a result object instead of the alternatives:
//   - A plain `bool` would tell the Controller THAT something failed, but
//     not WHY - and the View still needs a human-readable reason to show
//     the user (Task 4.5's ShowMessage(...)).
//   - Throwing an exception for an expected, everyday outcome like "that
//     roll number is already taken" would force the Controller to wrap
//     every call in try/catch just to keep the app running. That drags a
//     decision (what counts as a "handled" failure) into the Controller,
//     which this week's objective says should do orchestration only.
// A Result object lets the Service hand back a plain piece of DATA - true
// or false, plus a message - that the Controller can forward to the View
// without inspecting or branching on it at all.
public class OperationResult
{
    public bool Success { get; }
    public string Message { get; }

    private OperationResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public static OperationResult Ok(string message) => new(true, message);

    public static OperationResult Fail(string message) => new(false, message);
}
