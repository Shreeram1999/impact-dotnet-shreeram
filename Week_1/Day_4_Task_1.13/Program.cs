// ===== var: type is inferred ONCE at compile time and then LOCKED =====
var count = 5;   // compiler infers "count" is an int, permanently
// The line below does NOT compile if uncommented:
//
//     count = "hello";
//
// Compiler error CS0029: "Cannot implicitly convert type 'string' to 'int'"
// "var" is NOT a dynamic/loosely-typed keyword — it's just a shorthand that
// asks the compiler to infer the static type from the initializer. After
// that, "count" is exactly as strongly typed as if you'd written "int count".
Console.WriteLine($"var count = {count} (type locked to {count.GetType().Name} at compile time)");

// ===== dynamic: type is resolved at RUNTIME, and can change =====
dynamic value = "hello";
Console.WriteLine($"value = \"{value}\", runtime type = {value.GetType()}");

value = 5;
Console.WriteLine($"value = {value}, runtime type = {value.GetType()}");

value = true;
Console.WriteLine($"value = {value}, runtime type = {value.GetType()}");
// Unlike "var", each assignment to a "dynamic" variable can hold a
// completely different type — the compiler skips type-checking entirely and
// defers everything to the runtime. That flexibility comes at a cost: typos
// in member names, wrong operators, etc. only surface as a RuntimeBinderException
// when that line actually executes, not as a compile error.

// ===== dynamic parameters =====
Console.WriteLine($"Add(3, 4) = {Add(3, 4)}");                 // int + int -> addition: 7
Console.WriteLine($"Add(\"3\", \"4\") = {Add("3", "4")}");     // string + string -> concatenation: "34"

static dynamic Add(dynamic a, dynamic b)
{
    // "+" is resolved at runtime based on whatever types a and b actually are
    // at the moment this executes — that's why ints add numerically but
    // strings concatenate, using the exact same line of code.
    return a + b;
}
