using System.Reflection;

// A generic validator: it works on ANY object, not just User specifically,
// by using reflection to discover which properties are tagged with
// [MaxLengthNo] at runtime and checking each one. This is the same trick
// real .NET validation libraries (like the built-in DataAnnotations,
// or ASP.NET model validation) use under the hood.
public static class Validator
{
    // Returns the list of warning messages instead of printing directly,
    // so both Program.cs and automated tests can inspect exactly what was
    // (or wasn't) flagged.
    public static List<string> Validate(object target)
    {
        var warnings = new List<string>();
        var type = target.GetType();

        foreach (var property in type.GetProperties())
        {
            // GetCustomAttribute<T>() looks for one specific attribute
            // type on this property and returns null if it isn't there -
            // so properties with no [MaxLengthNo] tag are simply skipped.
            var maxLengthAttribute = property.GetCustomAttribute<MaxLengthNoAttribute>();
            if (maxLengthAttribute is null)
                continue;

            // GetValue(target) reads the CURRENT value of this property
            // off the specific object we were given - the reflection
            // equivalent of writing `target.Name`, except we don't know
            // the property is called "Name" until we discover it here.
            var value = property.GetValue(target) as string;

            if (value is not null && value.Length > maxLengthAttribute.MaxLength)
            {
                warnings.Add(
                    $"{type.Name}.{property.Name} is {value.Length} characters long, " +
                    $"exceeding the maximum of {maxLengthAttribute.MaxLength}.");
            }
        }

        return warnings;
    }
}
