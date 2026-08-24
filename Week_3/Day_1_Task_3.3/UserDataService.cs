// Task 3.3 - async/await.
public static class UserDataService
{
    // `async Task<string>` means: this method runs asynchronously, and
    // when it eventually finishes it hands back a string. Inside, `await`
    // is the keyword that actually does the "asynchronous" part: when we
    // hit `await Task.Delay(...)`, this method PAUSES and gives control
    // back to whoever called it, WITHOUT blocking the thread it's running
    // on. The thread is free to go do other work in the meantime. Once
    // the delay finishes, execution resumes right where it left off.
    //
    // Task.Delay(3000) simulates a slow operation - like a real network
    // call to fetch a user's data from a server - without actually needing
    // a network. In a real app this line would be something like
    // `await httpClient.GetStringAsync(...)`.
    public static async Task<string> FetchUserDataAsync(int userId)
    {
        Console.WriteLine($"[User {userId}] Starting fetch...");

        await Task.Delay(3000);

        Console.WriteLine($"[User {userId}] Fetch complete.");
        return $"Data for user {userId}";
    }
}
