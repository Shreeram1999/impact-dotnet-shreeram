// ===== Part 1: implicit numeric conversions (widening — always safe) =====
// Each step below moves to a type that can represent every value the
// previous type could, so the compiler allows it with no cast and no data loss.
int intValue = 42;
long longValue = intValue;      // int -> long: no cast needed
float floatValue = longValue;   // long -> float: no cast needed
double doubleValue = floatValue; // float -> double: no cast needed
Console.WriteLine($"int={intValue} -> long={longValue} -> float={floatValue} -> double={doubleValue}");

// ===== Part 2: explicit conversion (narrowing — can lose data) =====
double preciseValue = 9.87;
int truncated = (int)preciseValue;   // requires an explicit cast: the compiler
                                      // is warning you "this can lose information"
Console.WriteLine($"double {preciseValue} -> (int) = {truncated}   <-- fraction (.87) is DISCARDED, not rounded");

// ===== Part 3: is / as on an object =====
object boxedValue = "12345";

// "is" checks the runtime type and, in its pattern-matching form, also
// extracts it into a new variable in one step. It's the right tool when you
// need to branch on "what type is this really?"
if (boxedValue is string textFromIs)
{
    Console.WriteLine($"'is' pattern matched: \"{textFromIs}\"");
}

// "as" attempts a reference conversion and yields null on failure instead of
// throwing. Right tool when failure is a normal, expected outcome and you'd
// rather check for null than catch an exception.
string? textFromAs = boxedValue as string;
Console.WriteLine($"'as' cast result: {(textFromAs is null ? "null (not a string)" : textFromAs)}");

object boxedNumber = 42; // NOT a string
string? failedAs = boxedNumber as string; // no exception, just null
Console.WriteLine($"'as' on non-string object: {(failedAs is null ? "null (safe failure)" : failedAs)}");

// ===== Part 4: parsing a string into a number =====
string goodNumber = "150";
string badNumber = "not-a-number";

// Convert.ToInt32: throws FormatException on bad input, but treats a NULL
// string as 0 instead of throwing (unlike int.Parse). Use it when you expect
// valid input and want an exception on genuinely bad data.
int converted = Convert.ToInt32(goodNumber);
Console.WriteLine($"Convert.ToInt32(\"{goodNumber}\") = {converted}");

// int.TryParse: NEVER throws. Returns true/false and gives you the parsed
// value via 'out'. Use it whenever the input is untrusted (user input, file
// data, etc.) and a parse failure is a normal case you want to handle, not
// an exception to catch.
if (int.TryParse(goodNumber, out int parsedGood))
{
    Console.WriteLine($"TryParse(\"{goodNumber}\") succeeded -> {parsedGood}");
}

if (!int.TryParse(badNumber, out int parsedBad))
{
    Console.WriteLine($"TryParse(\"{badNumber}\") failed safely -> default value {parsedBad}");
}
