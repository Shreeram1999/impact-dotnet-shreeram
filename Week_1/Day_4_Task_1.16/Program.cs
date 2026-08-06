// ---- Parameterized constructor ----
var alice = new Student("Alice", 21);
Console.WriteLine($"{alice.Name}, age {alice.Age}, enrolled {alice.EnrollmentDate:d}");

// ---- Chained constructor (defaults age to 18 via ": this(name, 18)") ----
var bob = new Student("Bob");
Console.WriteLine($"{bob.Name}, age {bob.Age} (defaulted via constructor chaining)");

// ---- Validation: age outside 5-100 is rejected ----
try
{
    var invalid = new Student("Charlie", 150);
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Validation rejected age 150: {ex.Message}");
}

// ---- Overloaded CalculateGrade ----
Console.WriteLine($"Single score 88 -> grade {alice.CalculateGrade(88)}");
Console.WriteLine($"Score array [70, 95, 60] -> grade {alice.CalculateGrade(new[] { 70, 95, 60 })}");

// ---- const vs readonly: what can/can't change, and where ----
Console.WriteLine($"Student.MinAge (const) = {Student.MinAge}"); // accessed via the TYPE, not an instance
// Student.MinAge = 10;
//   -> CS0131: "The left-hand side of an assignment must be a variable,
//      property or indexer" — a const is a compile-time literal baked into
//      every call site; it cannot be reassigned ANYWHERE, ever.

// alice.EnrollmentDate = DateTime.Today;
//   -> CS0191: "A readonly field cannot be assigned to (except in a
//      constructor or init-only setter of the type in which the field is
//      defined)". readonly CAN be set inside Student's own constructor
//      (that's how EnrollmentDate got its value) but not from outside, and
//      not from inside Student after construction either.
