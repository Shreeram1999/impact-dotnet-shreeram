// EventArgs subclass carrying the info subscribers actually need to know -
// same pattern as Task 2.9's AlarmEventArgs.
public class NotificationSentEventArgs : EventArgs
{
    public string Recipient { get; }
    public string Message { get; }

    public NotificationSentEventArgs(string recipient, string message)
    {
        Recipient = recipient;
        Message = message;
    }
}

// NotificationService doesn't hardcode "send by email" or "send by SMS" -
// instead, the actual delivery method is passed IN as a NotificationSender
// delegate through the constructor (this pattern is called "dependency
// injection": the class depends on a delegate/interface, not on a
// specific concrete implementation). That's what lets the SAME
// NotificationService class work for email, SMS, and push in Program.cs
// below, just by constructing it three times with three different senders.
public class NotificationService
{
    private readonly NotificationSender sender;

    // Just like Task 2.9's AlarmClock, this is an `event` (not a plain
    // delegate field), so outside code can only subscribe/unsubscribe with
    // += and -=. Whenever Send() below successfully delivers a message, it
    // "raises" (fires) this event so anyone listening finds out.
    public event EventHandler<NotificationSentEventArgs>? OnNotificationSent;

    public NotificationService(NotificationSender sender)
    {
        this.sender = sender;
    }

    public void Send(string recipient, string message)
    {
        sender(recipient, message);
        // `?.Invoke` only fires the event if at least one subscriber has
        // signed up with += - otherwise OnNotificationSent is null and
        // this line safely does nothing instead of crashing.
        OnNotificationSent?.Invoke(this, new NotificationSentEventArgs(recipient, message));
    }
}

// A concrete example of "someone reacting to the event". Program.cs wires
// this up with `service.OnNotificationSent += log.Record;` - NotificationLog
// and NotificationService never need to know each other's full type, only
// that Record() matches the EventHandler<NotificationSentEventArgs> shape.
public class NotificationLog
{
    public List<string> Entries { get; } = new();

    public void Record(object? sender, NotificationSentEventArgs e)
    {
        var entry = $"Sent to {e.Recipient}: {e.Message}";
        Entries.Add(entry);
        Console.WriteLine($"[Log] {entry}");
    }
}
