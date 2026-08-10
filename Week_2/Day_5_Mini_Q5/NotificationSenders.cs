// Mini Project Q5 - Notification Engine.
// `NotificationSender` is a custom delegate type (same idea as Task 2.8's
// MathOperation): it describes any method shaped like
// `void SomeName(string recipient, string message)`. EmailSender, SmsSender
// and PushSender below all match that shape, so any one of them can be
// "plugged into" a NotificationSender variable - see NotificationService.cs.
public delegate void NotificationSender(string recipient, string message);

public static class EmailSender
{
    public static void Send(string recipient, string message) =>
        Console.WriteLine($"[Email to {recipient}] {message}");
}

public static class SmsSender
{
    public static void Send(string recipient, string message) =>
        Console.WriteLine($"[SMS to {recipient}] {message}");
}

public static class PushSender
{
    public static void Send(string recipient, string message) =>
        Console.WriteLine($"[Push to {recipient}] {message}");
}
