var validUser = new User { Name = "Asha" };
var warningsForValidUser = Validator.Validate(validUser);
Console.WriteLine($"User \"{validUser.Name}\" (within limit): {warningsForValidUser.Count} warning(s).");

var invalidUser = new User { Name = "This Name Is Way Too Long" };
var warningsForInvalidUser = Validator.Validate(invalidUser);
Console.WriteLine($"User \"{invalidUser.Name}\" (over limit): {warningsForInvalidUser.Count} warning(s).");
foreach (var warning in warningsForInvalidUser)
    Console.WriteLine($"  WARNING: {warning}");
