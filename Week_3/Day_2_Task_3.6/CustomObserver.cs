// Task 3.6 - the Observer pattern, version 1: a hand-written interface.
//
// "Observer" means: one object (the "subject", here StockTickerCustom)
// keeps a list of other objects (the "observers", here Investors) that
// want to know whenever something changes. When the change happens, the
// subject loops through its list and notifies each one. This is the same
// underlying idea as Week 2's events (AlarmClock/NotificationService), but
// here we build it ourselves with a plain interface instead of using C#'s
// built-in `event` keyword - Task3_6_Events.cs (further down) redoes the
// exact same scenario using `event` for comparison.
//
// Note: we're calling this IStockObserver rather than IObserver, to avoid
// clashing with .NET's own built-in System.IObserver<T> interface (used by
// Reactive Extensions) - but the idea is exactly what the task means by "a
// custom IObserver interface": one we define ourselves, not a built-in one.
public interface IStockObserver
{
    void OnPriceChanged(string symbol, decimal newPrice);
}

public class Investor : IStockObserver
{
    public string Name { get; }
    public Investor(string name) => Name = name;

    public void OnPriceChanged(string symbol, decimal newPrice)
    {
        Console.WriteLine($"[Custom] {Name} notified: {symbol} is now {newPrice:C}");
    }
}

public class StockTickerCustom
{
    // The subject keeps its own list of observers, and controls entirely
    // when/how they get notified - unlike C#'s `event`, there's no
    // language-level protection here stopping other code from directly
    // messing with the observers list if it can get a reference to it (we
    // avoid that by keeping the list private and only exposing Attach/Detach).
    private readonly List<IStockObserver> observers = new();

    private decimal price;

    public void Attach(IStockObserver observer) => observers.Add(observer);
    public void Detach(IStockObserver observer) => observers.Remove(observer);

    public void SetPrice(string symbol, decimal newPrice)
    {
        price = newPrice;

        // Manually loop through every observer and call its method - this
        // is exactly what C#'s `event` mechanism does automatically behind
        // the scenes when you write `SomeEvent?.Invoke(...)`.
        foreach (var observer in observers)
            observer.OnPriceChanged(symbol, price);
    }
}
