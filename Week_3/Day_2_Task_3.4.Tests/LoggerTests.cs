// The whole point of a Singleton is "always the same instance" - so every
// test here is really just some variation of "ask for Logger.Instance
// twice (or more), and make sure both answers are literally the same object".
public class LoggerTests
{
    [Fact]
    public void Instance_CalledTwice_ReturnsSameObject()
    {
        var first = Logger.Instance;
        var second = Logger.Instance;

        Assert.Same(first, second);
    }

    [Fact]
    public void Instance_CalledManyTimes_AlwaysReturnsSameHashCode()
    {
        var hashCodes = Enumerable.Range(0, 10)
            .Select(_ => Logger.Instance.GetHashCode())
            .Distinct()
            .ToList();

        // If every call returned the same object, Distinct() should have
        // collapsed all 10 hash codes down to just 1.
        Assert.Single(hashCodes);
    }

    [Fact]
    public async Task Instance_AccessedFromMultipleTasks_AllSeeTheSameInstance()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => Logger.Instance))
            .ToArray();

        var instances = await Task.WhenAll(tasks);

        Assert.All(instances, instance => Assert.Same(Logger.Instance, instance));
    }
}
