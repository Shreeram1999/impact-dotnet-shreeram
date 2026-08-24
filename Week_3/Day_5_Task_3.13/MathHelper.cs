// Task 3.13 - static vs instance methods, part 1: MathHelper.
//
// `static` means these methods belong to the CLASS itself, not to any
// particular object - you call them as `MathHelper.Factorial(5)`, never
// `new MathHelper().Factorial(5)`. That fits here because none of these
// methods need any REMEMBERED state between calls - each one just takes
// some numbers in and returns an answer, exactly like the built-in
// `Math.Sqrt(...)` or `Math.Max(...)`. There would be no benefit to
// creating a "MathHelper object" first; there's nothing for it to hold on to.
public static class MathHelper
{
    // Factorial(n) = n x (n-1) x (n-2) x ... x 1. By definition,
    // Factorial(0) = 1 (an empty product) and Factorial(1) = 1.
    // Factorial isn't meaningful for negative numbers, so we throw instead
    // of returning a made-up answer.
    public static long Factorial(int n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "Factorial is not defined for negative numbers.");

        long result = 1;
        for (var i = 2; i <= n; i++)
            result *= i;

        return result;
    }

    // A prime number is a whole number greater than 1 that has no
    // divisors other than 1 and itself. That means 0, 1, and every
    // negative number are simply NOT prime by definition - no exception
    // needed here, `false` is already the mathematically correct answer.
    public static bool IsPrime(int n)
    {
        if (n < 2)
            return false;

        // We only need to check divisors up to the square root of n - if
        // n had a divisor larger than its square root, it would have to
        // pair with a divisor smaller than the square root too, and we'd
        // have already found that smaller one first.
        for (var divisor = 2; divisor * divisor <= n; divisor++)
        {
            if (n % divisor == 0)
                return false;
        }

        return true;
    }

    // GCD = Greatest Common Divisor: the largest number that evenly
    // divides both a and b. This uses the classic Euclidean algorithm:
    // repeatedly replace the larger number with the remainder of dividing
    // it by the smaller one, until one of them reaches zero.
    public static int GCD(int a, int b)
    {
        // Math.Abs so negative inputs behave the same as their positive
        // equivalents (the GCD of -12 and 8 is the same as GCD of 12 and 8).
        a = Math.Abs(a);
        b = Math.Abs(b);

        while (b != 0)
        {
            (a, b) = (b, a % b);
        }

        // GCD(0, 0) is mathematically undefined (every number divides 0,
        // so there's no single "greatest" one) - we surface that instead
        // of silently returning 0 as if it were a real answer.
        if (a == 0)
            throw new ArgumentException("GCD is not defined when both numbers are zero.");

        return a;
    }
}
