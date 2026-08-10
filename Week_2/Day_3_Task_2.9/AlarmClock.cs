// Task 2.9 - events.
// An "event" is .NET's standard pattern for "something happened, and other
// classes might want to react to it". It's built on top of delegates: an
// event is essentially a specially-protected delegate.

// EventArgs is the base class .NET expects for "data describing what
// happened". We inherit from it and add our own AlarmTime field, since
// subscribers need to know WHEN the alarm went off, not just THAT it did.
public class AlarmEventArgs : EventArgs
{
    public DateTime AlarmTime { get; }
    public AlarmEventArgs(DateTime alarmTime) => AlarmTime = alarmTime;
}

public class AlarmClock
{
    // `EventHandler<AlarmEventArgs>` is .NET's standard shape for an event
    // handler method: `void SomeMethod(object? sender, AlarmEventArgs e)`.
    // Declaring this with the `event` keyword (rather than as a plain
    // public delegate field) means outside code can only subscribe (+=) or
    // unsubscribe (-=) - it CANNOT call OnAlarmRing directly, and it can't
    // wipe out other subscribers' handlers by accident. Only AlarmClock
    // itself (via RingAlarm below) is allowed to actually fire it.
    public event EventHandler<AlarmEventArgs>? OnAlarmRing;

    // `?.Invoke(...)` is a safety check: if nobody has subscribed to
    // OnAlarmRing yet, it's `null`, and calling Invoke() on null would
    // normally crash the program. The `?.` means "only call Invoke if this
    // isn't null", so RingAlarm() is safe to call even with zero listeners.
    public void RingAlarm(DateTime alarmTime)
    {
        OnAlarmRing?.Invoke(this, new AlarmEventArgs(alarmTime));
    }
}

// Person and CoffeeMachine have NOTHING to do with each other, and neither
// one needs to know AlarmClock exists at compile time beyond matching the
// EventHandler<AlarmEventArgs> method signature. That's the point of
// events: totally decoupled classes can still react to the same trigger.
public class Person
{
    public string Name { get; }
    public Person(string name) => Name = name;

    public void WakeUp(object? sender, AlarmEventArgs e)
    {
        Console.WriteLine($"{Name} wakes up at {e.AlarmTime:HH:mm}.");
    }
}

public class CoffeeMachine
{
    public void StartBrewing(object? sender, AlarmEventArgs e)
    {
        Console.WriteLine($"CoffeeMachine starts brewing at {e.AlarmTime:HH:mm}.");
    }
}
