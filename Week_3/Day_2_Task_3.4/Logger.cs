// Task 3.4 - the Singleton pattern.
//
// "Singleton" means: no matter how many times or from how many places you
// ask for this class, you always get back the exact SAME object - never a
// second, independent copy. This is useful for things like a logger, where
// having two different Logger objects floating around, each with their own
// state, would be confusing or wasteful.
//
// `sealed` means no other class is allowed to inherit from Logger - that's
// intentional for a Singleton, since a subclass could otherwise be used to
// create a "different kind" of Logger, breaking the "always exactly one
// instance" guarantee.
public sealed class Logger
{
    // `Lazy<T>` is a built-in .NET wrapper that delays creating the object
    // inside it until the FIRST time something actually asks for `.Value`.
    // Passing `true` as the second constructor argument here means "make
    // this thread-safe" - if two threads ask for `.Value` at almost the
    // exact same moment, Lazy<T> guarantees only ONE Logger actually gets
    // constructed, and both threads end up with the same one. Without
    // that thread-safety, two threads racing each other could each create
    // their own separate Logger, which would break the whole point of a
    // Singleton.
    private static readonly Lazy<Logger> LazyInstance =
        new(() => new Logger(), isThreadSafe: true);

    // The single, shared instance everyone should use. `static` means it
    // belongs to the Logger CLASS itself, not to any particular object -
    // you access it as `Logger.Instance`, not `someLoggerVariable.Instance`.
    public static Logger Instance => LazyInstance.Value;

    // A `private` constructor is the key trick that makes this a real
    // Singleton: it stops any OTHER code (even in a different file) from
    // ever writing `new Logger()`. The only way to get a Logger is through
    // the Instance property above.
    private Logger()
    {
    }

    public void Log(string message)
    {
        // GetHashCode() gives us a number that (in practice, for a normal
        // object like this) uniquely identifies THIS object in memory. If
        // every call to Log(), from every thread and every task, prints
        // the same hash code, that's our proof they're all talking to the
        // one and only Logger instance.
        Console.WriteLine($"[Logger #{GetHashCode()}] {message}");
    }
}
