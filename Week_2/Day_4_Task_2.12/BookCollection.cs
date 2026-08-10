using System.Collections;

// A `record` is a compact way to define a simple data-holding type - C#
// auto-generates the constructor, a nice ToString(), and value-based
// equality for you, so you don't have to write all that boilerplate by hand.
public record Book(string Title, string Author);

// Implementing IEnumerable<Book> is exactly what makes it legal to write
// `foreach (var book in someBookCollection)` on this class - `foreach` only
// works on types that implement IEnumerable (or IEnumerable<T>).
public class BookCollection : IEnumerable<Book>
{
    private readonly List<Book> books = new();

    // Having a public Add(Book) method is also what unlocks the
    // "collection initializer" syntax used in Program.cs:
    //   new BookCollection { book1, book2, book3 }
    // C# just calls Add() once per item listed inside the braces.
    public void Add(Book book) => books.Add(book);

    // This is another `yield return` iterator, just like Task 2.12's
    // GetEvenNumbers - it sorts the books by title AS it's being consumed,
    // rather than sorting everything up front. Notice the underlying
    // `books` list itself never gets reordered - it stays in whatever
    // order items were Add()-ed; only what you get back from foreach
    // comes out sorted.
    public IEnumerator<Book> GetEnumerator()
    {
        foreach (var book in books.OrderBy(b => b.Title, StringComparer.Ordinal))
            yield return book;
    }

    // IEnumerable<Book> technically requires this second, non-generic
    // GetEnumerator() too (an older, pre-generics version of the same
    // interface that .NET still requires for backward compatibility). It
    // just forwards to the generic one above - you'll see this pattern
    // any time you hand-write a custom collection type.
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
