using System.Globalization;

// Task 2.14 - extension methods.
// An extension method is a static method that LOOKS like it's an instance
// method on some existing type, even though you never modified that type's
// source code. The trick is two things: the method must live in a
// `static class`, and its first parameter must be prefixed with `this`.
// After that, C# lets you call it with dot-syntax, e.g.
// `"hello".ToTitleCase()` instead of `StringExtensions.ToTitleCase("hello")`
// - both do the exact same thing, the `this` version just reads nicer.
public static class StringExtensions
{
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        // .NET's built-in TitleCase converter only capitalizes a word's
        // first letter reliably if the REST of the word is lowercase - if
        // you feed it "HELLO" it leaves it as "HELLO" instead of "Hello".
        // So we lowercase everything first, then let it capitalize each
        // word's first letter.
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
    }
}

public static class ListExtensions
{
    // Notice the extended type is `List<T>?` - the `?` means "nullable".
    // That's what allows this extension method to be called even on a
    // variable that's currently null, e.g. `myNullList.IsNullOrEmpty()`
    // works and returns true, without throwing a NullReferenceException.
    // A REAL instance method could never be called on a null reference
    // like that - this is one of the special abilities extension methods have.
    public static bool IsNullOrEmpty<T>(this List<T>? list) => list is null || list.Count == 0;
}

public static class IntExtensions
{
    // Lookup tables for the irregular English number words (1-19) and the
    // regular "tens" words (20, 30, 40, ...) - avoids a big if/else chain.
    private static readonly string[] Ones =
    [
        "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
        "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
        "Seventeen", "Eighteen", "Nineteen",
    ];

    private static readonly string[] Tens =
    [
        "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety",
    ];

    public static string ToWords(this int number)
    {
        if (number is < 0 or > 999)
            throw new ArgumentOutOfRangeException(nameof(number), "Only 0-999 is supported.");

        // 0 through 19 each have their own irregular name ("Eleven", not
        // "Ten-One"), so we just look them up directly in the Ones array.
        if (number < 20)
            return Ones[number];

        // 20-99: a regular tens word (Twenty, Thirty, ...), plus a ones
        // word tacked on with a hyphen if the last digit isn't zero
        // (e.g. 45 -> "Forty" + "-Five").
        if (number < 100)
        {
            var tensPart = Tens[number / 10];
            var onesPart = number % 10;
            return onesPart == 0 ? tensPart : $"{tensPart}-{Ones[onesPart]}";
        }

        // 100-999: say "<digit> Hundred", and if there's anything left
        // over (the last two digits), figure out its words by calling
        // ToWords() AGAIN on just that remainder - reusing the same
        // extension method recursively instead of duplicating the 0-99
        // logic a second time.
        var hundredsPart = $"{Ones[number / 100]} Hundred";
        var remainder = number % 100;
        return remainder == 0 ? hundredsPart : $"{hundredsPart} {remainder.ToWords()}";
    }
}
