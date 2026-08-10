// Each of the four required queries gets its own small method here, using
// LINQ method syntax (chained .Where/.GroupBy/.OrderBy calls). Splitting
// them into named methods (instead of writing the LINQ inline in
// Program.cs) also makes each one independently testable - see
// Day_5_Mini_Q6.Tests.
public static class LibraryQueries
{
    // "Available books by author": two conditions chained with `&&` inside
    // one Where() - a book only passes if BOTH IsAvailable is true AND the
    // Author matches.
    public static IEnumerable<Book> AvailableBooksByAuthor(IEnumerable<Book> books, string author) =>
        books.Where(b => b.IsAvailable && b.Author == author);

    // "Group by genre with count": GroupBy(b => b.Genre) buckets every book
    // by its Genre string. Each bucket (called `g` here) has a `.Key`
    // (the genre name) and behaves like a mini-list of books you can call
    // Count() on. Select(...) then turns each bucket into a simple
    // (Genre, Count) tuple.
    public static IEnumerable<(string Genre, int Count)> GroupByGenre(IEnumerable<Book> books) =>
        books
            .GroupBy(b => b.Genre)
            .Select(g => (Genre: g.Key, Count: g.Count()));

    // "Oldest book": sort every book by Year, smallest (earliest) first,
    // and take just the first one. FirstOrDefault() (rather than First())
    // returns null instead of throwing an exception if the collection
    // happens to be empty - safer to call on data you're not 100% sure
    // isn't empty.
    public static Book? OldestBook(IEnumerable<Book> books) =>
        books.OrderBy(b => b.Year).FirstOrDefault();

    // "Books after 2010 sorted by title": filter first (Where), then sort
    // what's left (OrderBy). `year` is a parameter here instead of a
    // hardcoded `2010` so this same method is reusable and testable for
    // any cutoff year, not just 2010.
    public static IEnumerable<Book> BooksAfter(IEnumerable<Book> books, int year) =>
        books.Where(b => b.Year > year).OrderBy(b => b.Title, StringComparer.Ordinal);
}
