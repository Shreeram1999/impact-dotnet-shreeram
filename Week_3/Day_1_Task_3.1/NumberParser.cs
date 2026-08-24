public static class NumberParser
{
    // A method with THREE catch blocks, in order from MOST specific to
    // LEAST specific. C# checks catch blocks top-to-bottom and runs the
    // FIRST one that matches the exception's type - so the order matters a
    // lot. FormatException and OverflowException are both more specific
    // (narrower) types than Exception (the base class every exception
    // eventually inherits from), so they have to come first, or they would
    // never get a chance to run.
    public static int ParseStrictly(string input)
    {
        try
        {
            // int.Parse can throw FormatException if the text isn't a
            // number at all (e.g. "abc"), or OverflowException if it IS a
            // number but too big/small to fit in an int (e.g. "99999999999999").
            return int.Parse(input);
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Not a valid number: {ex.Message}");
            throw;
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"Number too large or too small for an int: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            // A catch-all for anything unexpected we didn't specifically
            // plan for. It has to be LAST, because Exception is the base
            // class of every possible exception - if it came first, it
            // would swallow FormatException and OverflowException too,
            // and they would never be reached.
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }
}

// What happens if you put the general Exception catch FIRST? Try
// uncommenting this method locally - it will not compile:
//
// public static int ParseStrictly_WrongOrder(string input)
// {
//     try
//     {
//         return int.Parse(input);
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Unexpected error: {ex.Message}");
//         throw;
//     }
//     catch (FormatException ex)   // <-- compile error happens here
//     {
//         Console.WriteLine($"Not a valid number: {ex.Message}");
//         throw;
//     }
// }
//
// CS0160: A previous catch clause already catches all exceptions of this
// or of a super type ('System.Exception'). The compiler is telling you:
// "the catch(Exception) block above would already have handled this, so
// your catch(FormatException) block can never run - that's dead code, and
// I'm not going to let you write it."
