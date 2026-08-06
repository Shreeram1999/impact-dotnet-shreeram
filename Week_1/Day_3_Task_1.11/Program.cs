// int? (shorthand for Nullable<int>) can hold either a valid int OR "no value".
int? maybeAge = null;
Console.WriteLine($"maybeAge.HasValue = {maybeAge.HasValue}");   // False - no value stored

maybeAge = 25;
Console.WriteLine($"maybeAge.HasValue = {maybeAge.HasValue}, Value = {maybeAge.Value}"); // True, 25

// ---- ApplyDiscount: null means "use the default 5%" ----
double priceA = ApplyDiscount(200.0, null);       // no discount specified -> default applies
double priceB = ApplyDiscount(200.0, 0.20);       // explicit 20% discount

Console.WriteLine($"No discount specified -> final price: {priceA:C}"); // 5% off 200 = 190
Console.WriteLine($"20% discount specified -> final price: {priceB:C}"); // 20% off 200 = 160

static double ApplyDiscount(double amount, double? discount)
{
    // "??" is the null-coalescing operator: if the left side is null, use the
    // right side instead. So "discount ?? 0.05" reads as "discount, or 5% if none given".
    double actualDiscount = discount ?? 0.05;
    return amount - (amount * actualDiscount);
}
