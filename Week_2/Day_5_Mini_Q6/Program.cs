// 16 sample books spanning 7 genres, several repeated authors (J.R.R.
// Tolkien appears three times), and a mix of available/checked-out copies
// - enough variety that each query below has real filtering/grouping work
// to do, instead of trivially returning "everything" or "nothing".
List<Book> books =
[
    new("The Hobbit", "J.R.R. Tolkien", "Fantasy", 1937, true),
    new("The Fellowship of the Ring", "J.R.R. Tolkien", "Fantasy", 1954, false),
    new("The Two Towers", "J.R.R. Tolkien", "Fantasy", 1954, true),
    new("Dune", "Frank Herbert", "Sci-Fi", 1965, true),
    new("Foundation", "Isaac Asimov", "Sci-Fi", 1951, true),
    new("I, Robot", "Isaac Asimov", "Sci-Fi", 1950, false),
    new("Project Hail Mary", "Andy Weir", "Sci-Fi", 2021, true),
    new("Clean Code", "Robert C. Martin", "Technology", 2008, true),
    new("The Pragmatic Programmer", "David Thomas", "Technology", 2019, true),
    new("Atomic Habits", "James Clear", "Self-Help", 2018, true),
    new("Deep Work", "Cal Newport", "Self-Help", 2016, false),
    new("Sapiens", "Yuval Noah Harari", "History", 2011, true),
    new("Guns, Germs, and Steel", "Jared Diamond", "History", 1997, true),
    new("Gone Girl", "Gillian Flynn", "Thriller", 2012, true),
    new("The Silent Patient", "Alex Michaelides", "Thriller", 2019, false),
    new("Educated", "Tara Westover", "Memoir", 2018, true),
];

Console.WriteLine("Available books by J.R.R. Tolkien:");
foreach (var b in LibraryQueries.AvailableBooksByAuthor(books, "J.R.R. Tolkien"))
    Console.WriteLine($"  {b.Title} ({b.Year})");

Console.WriteLine();
Console.WriteLine("Book count by genre:");
foreach (var g in LibraryQueries.GroupByGenre(books))
    Console.WriteLine($"  {g.Genre}: {g.Count}");

Console.WriteLine();
var oldest = LibraryQueries.OldestBook(books);
// `oldest?.Title` uses the null-conditional operator - if OldestBook()
// somehow returned null, this just prints nothing instead of crashing
// with a NullReferenceException.
Console.WriteLine($"Oldest book: {oldest?.Title} ({oldest?.Year})");

Console.WriteLine();
Console.WriteLine("Books published after 2010, sorted by title:");
foreach (var b in LibraryQueries.BooksAfter(books, 2010))
    Console.WriteLine($"  {b.Title} ({b.Year})");
