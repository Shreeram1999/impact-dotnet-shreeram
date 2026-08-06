// #define MUST be the very first thing in the file — before any "using"
// directive or any other code — otherwise you get CS1032:
//   "Cannot define/undefine preprocessor symbols after first token in file".
// It defines a preprocessor symbol that only exists at COMPILE time; it has
// nothing to do with C# variables and doesn't survive into the compiled IL.
#define TRIAL_VERSION

// Comment out the line above (or change to a symbol that isn't defined) and
// rebuild to see the output switch to the "#else" branch.

#if TRIAL_VERSION
Console.WriteLine("Running TRIAL version — some features are limited.");
#else
Console.WriteLine("Running FULL version — all features unlocked.");
#endif

var config = new AppConfig("Contoso Trial App", 30);
config.PrintSummary();
