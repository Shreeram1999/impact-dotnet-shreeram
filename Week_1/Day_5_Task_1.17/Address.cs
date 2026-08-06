// A positional record. The compiler auto-generates: a constructor from the
// parameter list, read-only (init-only) properties for each parameter,
// value-based Equals/GetHashCode/==, ToString(), and a Deconstruct method.
public record Address(string Street, string City, string Pincode);
