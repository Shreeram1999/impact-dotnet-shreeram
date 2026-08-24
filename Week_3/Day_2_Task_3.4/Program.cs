// We'll hit Logger.Instance from two DIFFERENT ways of running code
// concurrently in .NET: raw Threads (the low-level building block) and
// Tasks (the higher-level, more modern way most C# code uses today).
// Either way, Logger.Instance should hand back the SAME object every time.

var threads = new List<Thread>();
for (var i = 1; i <= 5; i++)
{
    var threadNumber = i; // capture the loop variable so each thread prints its own number
    var thread = new Thread(() => Logger.Instance.Log($"Hello from Thread {threadNumber}"));
    threads.Add(thread);
    thread.Start();
}

// Thread.Join() blocks until that specific thread has finished running -
// we wait for all 5 before moving on, so their output doesn't overlap with
// the Tasks below.
foreach (var thread in threads)
    thread.Join();

var tasks = new List<Task>();
for (var i = 1; i <= 5; i++)
{
    var taskNumber = i;
    // Task.Run schedules the work to run on a background thread pulled
    // from the .NET thread pool, and immediately gives us back a Task we
    // can wait on later.
    tasks.Add(Task.Run(() => Logger.Instance.Log($"Hello from Task {taskNumber}")));
}

// Task.WaitAll blocks until every task in the array has completed.
Task.WaitAll(tasks.ToArray());

Console.WriteLine();
Console.WriteLine("Look at the [Logger #...] numbers above - they should all be identical,");
Console.WriteLine("proving every Thread and every Task got the exact same Logger instance.");
