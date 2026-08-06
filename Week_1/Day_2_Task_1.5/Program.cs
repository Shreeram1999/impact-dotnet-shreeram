using SchoolManagement; // enables the short "Student" name used below

// NOTE: a C# project can only have ONE entry point. Since this file uses
// top-level statements (no explicit "class Program { static void Main }"),
// this whole file *is* the Main method. That's why both call styles are
// demonstrated here in sequence instead of in two separate Main methods.

// ---- Call style 1: WITHOUT a using directive ----
// We must write the fully-qualified name every time: <Namespace>.<Type>.
SchoolManagement.Student studentA = new SchoolManagement.Student();
studentA.Name = "John";
studentA.Display();

// ---- Call style 2: WITH a using directive ----
// See the "using SchoolManagement;" line at the very top of the file.
// Once that's in scope, we can drop the namespace prefix and just say "Student".
Student studentB = new Student();
studentB.Name = "Alice";
studentB.Display();

// DIFFERENCE:
// - Without "using", the compiler needs the full path (Namespace.Type) every
//   time you reference the type. This avoids ambiguity but is verbose, and is
//   handy when you only need a type once or want to be explicit about where
//   it comes from (e.g. to disambiguate two types with the same name).
// - With "using SchoolManagement;" at the top of the file, the compiler
//   brings every public type in that namespace into scope, so you can use
//   the short name "Student" directly. This is shorter but can cause a name
//   clash if two "using"d namespaces both define a "Student" type
//   (see Task 1.6 for how to resolve that with fully-qualified names).
