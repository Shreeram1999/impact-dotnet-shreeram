// Task 2.3 - virtual/override, plus the `sealed` keyword.
public class Notification
{
    public virtual void Send(string message)
    {
        Console.WriteLine($"[Notification] {message}");
    }
}

public class EmailNotification : Notification
{
    // `sealed override` does two things at once: it overrides Send() like
    // normal, AND it locks the override chain so that NO class that
    // inherits from EmailNotification is allowed to override Send() again.
    // Regular `override` (see SmsNotification/PushNotification below)
    // would still allow further overriding down the line; `sealed` says
    // "this is the final word on Send() for this branch of the family tree".
    public sealed override void Send(string message)
    {
        Console.WriteLine($"[Email] {message}");
    }
}

// These two use plain `override` (no `sealed`), so unlike EmailNotification,
// a class inheriting from either of these COULD still override Send() again.
public class SmsNotification : Notification
{
    public override void Send(string message)
    {
        Console.WriteLine($"[SMS] {message}");
    }
}

public class PushNotification : Notification
{
    public override void Send(string message)
    {
        Console.WriteLine($"[Push] {message}");
    }
}

// This is what happens if you try to break the seal. It won't compile, so
// it's left here as a comment instead of real code - uncomment it locally
// if you want to see the compiler error for yourself:
//
// public class SecureEmailNotification : EmailNotification
// {
//     public override void Send(string message) => Console.WriteLine(message);
// }
//
// CS0239: 'SecureEmailNotification.Send(string)': cannot override inherited
// member 'EmailNotification.Send(string)' because it is sealed.
