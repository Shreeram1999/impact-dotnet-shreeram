// Every possible branch of IsNullOrEmpty() gets its own test: a null
// reference, an empty (but non-null) list, and a list with at least one
// item. Testing all three matters because a bug in just one branch
// (e.g. forgetting the null check) wouldn't show up if we only tested one case.
public class ListExtensionsTests
{
    [Fact]
    public void IsNullOrEmpty_NullList_ReturnsTrue()
    {
        List<int>? list = null;
        Assert.True(list.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_EmptyList_ReturnsTrue()
    {
        Assert.True(new List<int>().IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_ListWithItems_ReturnsFalse()
    {
        Assert.False(new List<int> { 1 }.IsNullOrEmpty());
    }
}
