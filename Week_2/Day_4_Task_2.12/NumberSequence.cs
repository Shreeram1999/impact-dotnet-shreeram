// Task 2.12 - `yield return` and lazy iteration.
public static class NumberSequence
{
    // This method returns IEnumerable<int> - "something you can loop over
    // with foreach" - but notice it never builds a List<int> to return.
    // Instead, `yield return i` is special C# syntax that turns this whole
    // method into an "iterator": the compiler rewrites it behind the
    // scenes into a mini state machine that remembers where it left off.
    //
    // The important part: NOTHING inside this method actually runs until
    // someone starts looping over the result (with foreach, or .ToList(),
    // etc). Each time the caller asks for "the next value", the loop below
    // resumes from wherever it paused, computes ONE more even number, and
    // pauses again at the next yield return. This is called "lazy
    // evaluation" - work happens only when it's actually needed, one item
    // at a time, instead of computing everything up front.
    public static IEnumerable<int> GetEvenNumbers(int max)
    {
        for (var i = 0; i <= max; i++)
        {
            if (i % 2 == 0)
                yield return i;
        }
    }
}
