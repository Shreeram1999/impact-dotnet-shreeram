// Task 3.10 - Thread vs Task.Run vs Parallel.ForEach (the Task Parallel
// Library, or "TPL").
public static class Worker
{
    // `Thread.Sleep` BLOCKS the thread it runs on - the thread just sits
    // there doing nothing useful for 100ms. This is different from
    // Task 3.3's `await Task.Delay(...)`, which frees up the thread while
    // waiting. We're using a blocking sleep here on purpose, because
    // Thread/Task.Run/Parallel.ForEach are all about running blocking (or
    // CPU-heavy) work on MULTIPLE threads at once, not about the
    // non-blocking async/await style from Task 3.3.
    public static void DoSimulatedWork(int id)
    {
        Thread.Sleep(100);
    }
}
