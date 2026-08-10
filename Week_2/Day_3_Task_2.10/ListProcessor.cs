// Task 2.10 - the three most common built-in delegate types, used together:
//   - Predicate<T>: a method that takes a T and returns bool (a yes/no test)
//   - Func<T,T,T>: a method that takes inputs and returns a value (a transform)
//   - Action<string>: a method that takes input but returns nothing (a
//     "do something with this" side effect, like printing)
// ProcessList below accepts all three as parameters, so the CALLER decides
// exactly what "filter", "transform", and "output" mean each time it's
// called - ProcessList itself doesn't hardcode any of that logic.
public static class ListProcessor
{
    public static void ProcessList(
        List<int> list,
        Predicate<int> filter,
        Func<int, int, int> transform,
        Action<string> output)
    {
        // `list.Where(x => filter(x))` keeps only the items where filter(x)
        // returns true. Then for each surviving item, we run it through
        // `transform` (passing the item twice - Program.cs uses this to
        // "square" the number by multiplying it by itself), and finally
        // hand the result to `output` to actually do something with it.
        foreach (var item in list.Where(x => filter(x)))
        {
            var transformed = transform(item, item);
            output(transformed.ToString());
        }
    }
}
