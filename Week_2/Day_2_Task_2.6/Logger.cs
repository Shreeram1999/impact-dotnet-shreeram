public class Logger
{
    public void Log(string message) => Console.WriteLine($"[Logger] {message}");
}

public class FileLogger : Logger
{
    // The `new` keyword here is called "method hiding" - it's easy to
    // confuse with `override`, but it behaves very differently:
    //   - `override` (see Task 2.2/2.3/2.4) means: whichever object this
    //     actually IS at runtime decides which method body runs.
    //   - `new` means: whichever TYPE the VARIABLE was DECLARED as decides
    //     which method body runs, checked at compile time, ignoring what
    //     the object actually is underneath.
    // Program.cs calls Log() through two different variables pointing at
    // the SAME FileLogger object to show how differently they behave.
    public new void Log(string message) => Console.WriteLine($"[FileLogger] {message}");
}
