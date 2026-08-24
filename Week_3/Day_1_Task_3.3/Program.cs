using System.Diagnostics;

// `Stopwatch` is a simple .NET class for timing how long something takes -
// StartNew() begins timing immediately, and .Elapsed gives you the time
// that's passed so far.

// --- Sequential: one fetch fully finishes before the next one starts ---
Console.WriteLine("=== Sequential (one at a time) ===");
var sequentialStopwatch = Stopwatch.StartNew();

// Each `await` here pauses THIS method until that one fetch is done,
// before even starting the next line - so three 3-second fetches take
// roughly 3 + 3 + 3 = 9 seconds in total.
await UserDataService.FetchUserDataAsync(1);
await UserDataService.FetchUserDataAsync(2);
await UserDataService.FetchUserDataAsync(3);

sequentialStopwatch.Stop();
Console.WriteLine($"Sequential total time: {sequentialStopwatch.Elapsed.TotalSeconds:F1}s");

Console.WriteLine();

// --- Concurrent: all three fetches start right away, running "at the same time" ---
Console.WriteLine("=== Concurrent (Task.WhenAll) ===");
var concurrentStopwatch = Stopwatch.StartNew();

// Calling FetchUserDataAsync WITHOUT awaiting it yet starts the work
// immediately and gives us back a Task we can hold onto. Doing this three
// times in a row kicks off all three fetches basically simultaneously.
var task1 = UserDataService.FetchUserDataAsync(1);
var task2 = UserDataService.FetchUserDataAsync(2);
var task3 = UserDataService.FetchUserDataAsync(3);

// Task.WhenAll waits for ALL of the given tasks to finish. Since they were
// all already running in parallel, the total wait is roughly as long as
// the SLOWEST one alone (~3 seconds), not the sum of all three.
await Task.WhenAll(task1, task2, task3);

concurrentStopwatch.Stop();
Console.WriteLine($"Concurrent total time: {concurrentStopwatch.Elapsed.TotalSeconds:F1}s");

Console.WriteLine();
Console.WriteLine($"Speedup: sequential took {sequentialStopwatch.Elapsed.TotalSeconds:F1}s, " +
                   $"concurrent took {concurrentStopwatch.Elapsed.TotalSeconds:F1}s.");
