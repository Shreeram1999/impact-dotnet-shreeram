// Each number here is generated one at a time, right as the foreach loop
// asks for it - not all ten computed up front into a hidden list. You
// can't see the laziness directly in the console output, but it's
// happening: GetEvenNumbers "pauses and resumes" between each printed line.
Console.WriteLine("Even numbers up to 10 (consumed lazily):");
foreach (var n in NumberSequence.GetEvenNumbers(10))
    Console.WriteLine($"  {n}");

// Books are listed here in a random-ish order (Hobbit, Atomic Habits,
// Clean Code, Dune) - but because BookCollection's own GetEnumerator()
// sorts them by title before yielding, the foreach below will print them
// alphabetically, regardless of the order they were added in.
var library = new BookCollection
{
    new Book("The Hobbit", "J.R.R. Tolkien"),
    new Book("Atomic Habits", "James Clear"),
    new Book("Clean Code", "Robert C. Martin"),
    new Book("Dune", "Frank Herbert"),
};

Console.WriteLine();
Console.WriteLine("Books alphabetically by title:");
foreach (var book in library)
    Console.WriteLine($"  {book.Title} - {book.Author}");
