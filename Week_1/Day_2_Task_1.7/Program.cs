// ---- 5 variables using correct C# naming conventions ----
// Convention: locals/parameters/fields -> camelCase. Types/methods -> PascalCase.
int studentAge = 21;            // camelCase local (int)
string firstName = "Alice";     // camelCase local (string)
double accountBalance = 1250.75; // camelCase local (double)
bool isEnrolled = true;         // camelCase local (bool)
DateTime enrollmentDate = DateTime.Today; // "DateTime" is a PascalCase TYPE name

Console.WriteLine($"{firstName} is {studentAge} years old, balance {accountBalance:C}, enrolled: {isEnrolled}, since {enrollmentDate:d}");

// A method name, by convention, is also PascalCase:
PrintDivider();

// ---- Trying to use "class" as a variable name ----
// The line below is commented out because it does NOT compile:
//
//     string class = "Physics101";
//
// Compiler error (CS1519 / CS1001 depending on version):
//   "A get, set, add, or remove accessor expected" / "Invalid token 'class'
//   in a member declaration" — because "class" is a C# RESERVED KEYWORD.
// The parser sees "class" and expects a class *declaration* to follow
// (e.g. "class Foo { }"), not a variable name, so it gets confused by the
// "= ..." that comes next.
//
// FIX: prefix the identifier with "@". The "@" tells the compiler
// "treat the next word as a plain identifier, not a keyword".
string @class = "Physics101";
Console.WriteLine($"Course: {@class}");

static void PrintDivider()
{
    Console.WriteLine(new string('+', 30));
}
