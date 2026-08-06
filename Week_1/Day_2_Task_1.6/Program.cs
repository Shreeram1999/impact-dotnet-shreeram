// If we added BOTH "using ModuleA;" and "using ModuleB;" here and then tried
// to call "Helper.Greet();", the compiler would fail with:
//   CS0104: 'Helper' is an ambiguous reference between 'ModuleA.Helper' and 'ModuleB.Helper'
// because both namespaces bring a type literally named "Helper" into scope
// and the compiler has no way to know which one you mean.
//
// The fix: call each one through its FULLY-QUALIFIED name (Namespace.Type),
// which sidesteps the clash entirely — no "using" needed for either.

ModuleA.Helper.Greet();
ModuleB.Helper.Greet();

// Alternative fix (not used here, just for reference): give one of them an
// alias so you can still use a short name for it:
//   using AHelper = ModuleA.Helper;
//   AHelper.Greet();
//   ModuleB.Helper.Greet();
