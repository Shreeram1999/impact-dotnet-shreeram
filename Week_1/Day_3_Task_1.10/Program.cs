// ===== Part 1: plain enum, number -> day name =====
// Enum members get sequential int values starting at 0 unless you say otherwise,
// so DaysOfWeek.Sunday == 0, Monday == 1, ... Saturday == 6.
int dayNumber = 3;
DaysOfWeek day = (DaysOfWeek)dayNumber;   // explicit cast: int -> enum
Console.WriteLine($"Day {dayNumber} is {day}");   // "day" formats as its name via ToString()

// ===== Part 2: [Flags] enum, bitwise combine and test =====
// Each member is a distinct BIT (1, 2, 4, ...) so they can be OR'd together
// without colliding, and AND'd to test whether a specific bit is set.
FilePermission userPerms = FilePermission.Read | FilePermission.Write;
Console.WriteLine($"Combined permissions: {userPerms}");   // prints "Read, Write" because of [Flags]

bool canWrite = (userPerms & FilePermission.Write) == FilePermission.Write;
bool canExecute = (userPerms & FilePermission.Execute) == FilePermission.Execute;
Console.WriteLine($"Can write? {canWrite}");     // true  - Write bit is set
Console.WriteLine($"Can execute? {canExecute}"); // false - Execute bit is NOT set

enum DaysOfWeek
{
    Sunday,    // 0
    Monday,    // 1
    Tuesday,   // 2
    Wednesday, // 3
    Thursday,  // 4
    Friday,    // 5
    Saturday   // 6
}

[Flags]
enum FilePermission
{
    None = 0,
    Read = 1,    // binary 001
    Write = 2,   // binary 010
    Execute = 4  // binary 100
}
