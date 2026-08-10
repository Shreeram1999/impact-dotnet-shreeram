// A smaller, 7-book sample (separate from Program.cs's bigger 16-book
// list) - just enough data to exercise every query's logic clearly:
// multiple genres to group, an author with one available and one
// unavailable book (to prove BOTH filter conditions are checked), an
// obvious oldest book, and a mix of pre-2010/post-2010 titles.
public class LibraryQueriesTests
{
    private static List<Book> SampleBooks() =>
    [
        new("The Hobbit", "J.R.R. Tolkien", "Fantasy", 1937, true),
        new("The Fellowship of the Ring", "J.R.R. Tolkien", "Fantasy", 1954, false),
        new("Dune", "Frank Herbert", "Sci-Fi", 1965, true),
        new("Foundation", "Isaac Asimov", "Sci-Fi", 1951, true),
        new("Project Hail Mary", "Andy Weir", "Sci-Fi", 2021, true),
        new("Clean Code", "Robert C. Martin", "Technology", 2008, true),
        new("Atomic Habits", "James Clear", "Self-Help", 2018, true),
    ];

    [Fact]
    public void AvailableBooksByAuthor_ReturnsOnlyAvailableBooksFromThatAuthor()
    {
        // Tolkien has TWO books in the sample, but one is checked out
        // (IsAvailable: false). If the query only checked the author and
        // forgot the availability filter, this test would catch it by
        // seeing 2 results instead of 1.
        var result = LibraryQueries.AvailableBooksByAuthor(SampleBooks(), "J.R.R. Tolkien").ToList();

        Assert.Single(result);
        Assert.Equal("The Hobbit", result[0].Title);
    }

    [Fact]
    public void AvailableBooksByAuthor_UnknownAuthor_ReturnsEmpty()
    {
        var result = LibraryQueries.AvailableBooksByAuthor(SampleBooks(), "Nobody");
        Assert.Empty(result);
    }

    [Fact]
    public void GroupByGenre_CountsBooksPerGenre()
    {
        var groups = LibraryQueries.GroupByGenre(SampleBooks()).ToDictionary(g => g.Genre, g => g.Count);

        Assert.Equal(2, groups["Fantasy"]);
        Assert.Equal(3, groups["Sci-Fi"]);
        Assert.Equal(1, groups["Technology"]);
        Assert.Equal(1, groups["Self-Help"]);
    }

    [Fact]
    public void OldestBook_ReturnsBookWithLowestYear()
    {
        var oldest = LibraryQueries.OldestBook(SampleBooks());

        Assert.NotNull(oldest);
        Assert.Equal("The Hobbit", oldest!.Title);
        Assert.Equal(1937, oldest.Year);
    }

    [Fact]
    public void OldestBook_EmptyCollection_ReturnsNull()
    {
        // Passing an empty list `[]` checks the FirstOrDefault() edge case
        // mentioned in LibraryQueries.cs - it should return null, not throw.
        Assert.Null(LibraryQueries.OldestBook([]));
    }

    [Fact]
    public void BooksAfter_ReturnsOnlyLaterBooksSortedByTitle()
    {
        var result = LibraryQueries.BooksAfter(SampleBooks(), 2010).ToList();

        Assert.Equal(["Atomic Habits", "Project Hail Mary"], result.Select(b => b.Title));
    }

    [Fact]
    public void BooksAfter_NoBooksMatch_ReturnsEmpty()
    {
        var result = LibraryQueries.BooksAfter(SampleBooks(), 3000);
        Assert.Empty(result);
    }
}
