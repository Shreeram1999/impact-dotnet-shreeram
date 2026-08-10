// `s => s.ToUpper()` style code is a "lambda expression" - a compact way
// to write a small method inline without giving it a name. Read
// `s => Console.WriteLine(s.ToUpper())` as "given some input called s, do
// Console.WriteLine(s.ToUpper())". Each lambda below matches one of the
// three delegate types ProcessList expects.
Action<string> printUppercase = s => Console.WriteLine(s.ToUpper());
Func<int, int, int> multiply = (a, b) => a * b;
Predicate<int> isEven = n => n % 2 == 0;

List<int> numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

// Expected pipeline, step by step:
//   1. isEven keeps only 2, 4, 6, 8, 10
//   2. multiply(item, item) squares each one: 4, 16, 36, 64, 100
//   3. printUppercase prints each one (numbers have no letters to
//      uppercase, so this step doesn't visibly change anything here, but
//      it proves the same Action<string> works regardless of content).
Console.WriteLine("Filter evens -> square -> print:");
ListProcessor.ProcessList(numbers, isEven, multiply, printUppercase);
