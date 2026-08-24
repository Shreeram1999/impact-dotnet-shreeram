using System.Diagnostics;

const int OperationCount = 100;

// --- Baseline: sequential foreach, one operation fully finishes before the next starts ---
Console.WriteLine("Sequential foreach...");
var sequentialStopwatch = Stopwatch.StartNew();
for (var i = 0; i < OperationCount; i++)
    Worker.DoSimulatedWork(i);
sequentialStopwatch.Stop();
Console.WriteLine($"  Sequential: {sequentialStopwatch.Elapsed.TotalSeconds:F2}s");

// --- Raw Thread objects: the lowest-level way to run code concurrently in .NET ---
// Each `new Thread(...)` asks the operating system for a brand-new OS
// thread, which is individually more expensive to create than a thread
// pool thread. But because all 100 are requested and started immediately
// in this loop, they all genuinely run in parallel right away - see the
// note after the results below for why that actually makes this the
// FASTEST option here, which is a bit surprising the first time you see it.
Console.WriteLine("Thread objects (one real OS thread per operation)...");
var threadStopwatch = Stopwatch.StartNew();
var threads = new List<Thread>();
for (var i = 0; i < OperationCount; i++)
{
    var id = i;
    var thread = new Thread(() => Worker.DoSimulatedWork(id));
    threads.Add(thread);
    thread.Start();
}
foreach (var thread in threads)
    thread.Join();
threadStopwatch.Stop();
Console.WriteLine($"  Thread: {threadStopwatch.Elapsed.TotalSeconds:F2}s");

// --- Task.Run: hands the work to .NET's thread pool instead of creating a
// brand-new OS thread each time. The thread pool REUSES a small set of
// threads across many short jobs, which is usually cheaper than Thread. ---
Console.WriteLine("Task.Run (thread pool)...");
var taskStopwatch = Stopwatch.StartNew();
var tasks = new List<Task>();
for (var i = 0; i < OperationCount; i++)
{
    var id = i;
    tasks.Add(Task.Run(() => Worker.DoSimulatedWork(id)));
}
Task.WaitAll(tasks.ToArray());
taskStopwatch.Stop();
Console.WriteLine($"  Task.Run: {taskStopwatch.Elapsed.TotalSeconds:F2}s");

// --- Parallel.ForEach: part of the Task Parallel Library (TPL). It's
// purpose-built for "run this same loop body over many items, spread
// across multiple threads" - it also uses the thread pool under the hood,
// but it manages splitting up the work for you, which is usually both
// simpler to write AND more efficient than managing 100 individual tasks
// yourself. ---
Console.WriteLine("Parallel.ForEach...");
var parallelStopwatch = Stopwatch.StartNew();
Parallel.ForEach(Enumerable.Range(0, OperationCount), Worker.DoSimulatedWork);
parallelStopwatch.Stop();
Console.WriteLine($"  Parallel.ForEach: {parallelStopwatch.Elapsed.TotalSeconds:F2}s");

Console.WriteLine();
Console.WriteLine($"Parallel.ForEach ({parallelStopwatch.Elapsed.TotalSeconds:F2}s) vs sequential " +
                   $"({sequentialStopwatch.Elapsed.TotalSeconds:F2}s): Parallel.ForEach is " +
                   $"{sequentialStopwatch.Elapsed.TotalSeconds / parallelStopwatch.Elapsed.TotalSeconds:F1}x faster.");

// A genuinely useful surprise you'll likely see in the numbers above:
// Thread often comes out FASTER than Task.Run/Parallel.ForEach for this
// specific kind of workload (many short, BLOCKING operations started all
// at once), even though creating 100 real OS threads is individually more
// expensive than reusing pooled ones. Why: the thread pool deliberately
// does NOT hand out 100 threads instantly - it starts with only a few and
// "injects" new ones gradually (roughly one every second or so) if the
// existing ones stay busy, to avoid flooding the system with threads for
// workloads that don't need them. Since Thread.Sleep blocks its thread
// completely, that slow ramp-up means Task.Run/Parallel.ForEach can end up
// waiting through several 100ms "waves" before every operation has a
// thread to run on - while raw Thread objects, which don't go through the
// pool at all, are all already running from the very first moment. This is
// exactly why real async I/O code should use `await` (Task 3.3, which
// doesn't block a thread at all) rather than Task.Run over blocking calls -
// Task.Run/Parallel.ForEach are the right tool for CPU-bound work spread
// across a SMALL number of items, not for launching a large burst of
// blocking operations all at once.
