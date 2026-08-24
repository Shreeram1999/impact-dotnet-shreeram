// Task 3.6 - the Observer pattern, version 2: C#'s built-in `event` keyword.
// This is the same StockTicker/Investor scenario as CustomObserver.cs,
// re-implemented using `event` (the same tool Week 2's AlarmClock and
// NotificationService used).
public class PriceChangedEventArgs : EventArgs
{
    public string Symbol { get; }
    public decimal NewPrice { get; }

    public PriceChangedEventArgs(string symbol, decimal newPrice)
    {
        Symbol = symbol;
        NewPrice = newPrice;
    }
}

public class StockTickerEvents
{
    // The `event` keyword gives us the same "notify a list of listeners"
    // behavior as CustomObserver.cs's `observers` list and foreach loop -
    // but the list-management and looping are handled FOR us by the
    // language/runtime, and outside code can only += / -= (subscribe or
    // unsubscribe), never call the event directly or clear other
    // subscribers by accident.
    public event EventHandler<PriceChangedEventArgs>? PriceChanged;

    public void SetPrice(string symbol, decimal newPrice)
    {
        PriceChanged?.Invoke(this, new PriceChangedEventArgs(symbol, newPrice));
    }
}

public class InvestorEventSubscriber
{
    public string Name { get; }
    public InvestorEventSubscriber(string name) => Name = name;

    public void OnPriceChanged(object? sender, PriceChangedEventArgs e)
    {
        Console.WriteLine($"[Event] {Name} notified: {e.Symbol} is now {e.NewPrice:C}");
    }
}

// Comparing the two approaches:
//   - CustomObserver.cs (our own IStockObserver interface) makes every
//     step explicit and visible: we can see the observers list, the
//     Attach/Detach methods, and the foreach loop that notifies everyone.
//     That's useful for LEARNING what "Observer" really does under the
//     hood, and it's how other languages without a built-in event system
//     (like Java) typically implement this pattern.
//   - EventObserver.cs (C#'s `event`) is shorter, and the compiler/runtime
//     enforces safety we'd otherwise have to write ourselves - e.g.
//     outside code cannot accidentally clear someone else's subscription,
//     and firing with zero subscribers is a simple null-check instead of
//     looping over an empty list. For real C# code, `event` is almost
//     always the better choice; the hand-written interface version is
//     mainly valuable for understanding the pattern itself.
