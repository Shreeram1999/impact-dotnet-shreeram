public class User
{
    // This is the attribute in use: "Name should never be longer than 10
    // characters." Just writing this doesn't enforce anything by itself -
    // it's Validator.cs, using reflection, that actually reads this tag
    // and does something with it.
    [MaxLengthNo(10)]
    public string Name { get; set; } = string.Empty;
}
