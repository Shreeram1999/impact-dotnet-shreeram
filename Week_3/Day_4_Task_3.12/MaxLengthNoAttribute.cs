// Task 3.12 - a custom attribute.
//
// Attributes are little tags you attach to code (classes, properties,
// methods, ...) using the [SquareBracket] syntax - you've probably already
// seen built-in ones like [Fact] in the test projects. An attribute by
// itself does NOTHING on its own; it's just a piece of metadata sitting on
// the property. It only becomes useful when some OTHER code uses
// reflection to go looking for it later - which is exactly what
// Validator.cs does below.
//
// `AttributeUsage` restricts where this attribute is allowed to be placed
// (only on properties, in our case) - trying to put [MaxLengthNo] on a
// class or a method would be a compile error.
[AttributeUsage(AttributeTargets.Property)]
public class MaxLengthNoAttribute : Attribute
{
    public int MaxLength { get; }

    public MaxLengthNoAttribute(int maxLength)
    {
        MaxLength = maxLength;
    }
}
