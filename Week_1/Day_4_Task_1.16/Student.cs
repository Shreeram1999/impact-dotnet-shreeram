public class Student
{
    // const: value fixed at COMPILE time, must be assigned right here, is
    // implicitly static (shared by the type, not per-instance), and can
    // NEVER be reassigned anywhere, not even in a constructor.
    public const int MinAge = 5;
    public const int MaxAge = 100;

    // readonly: value can be assigned either at declaration or inside a
    // constructor, but nowhere else afterward — it's a per-instance value
    // that becomes immutable once construction finishes.
    public readonly DateTime EnrollmentDate;

    public string Name { get; set; }

    private int _age;
    public int Age
    {
        get => _age;
        set
        {
            if (value < MinAge || value > MaxAge)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), $"Age must be between {MinAge} and {MaxAge}.");
            }
            _age = value;
        }
    }

    // Parameterized constructor — the "real" one that does the work.
    public Student(string name, int age)
    {
        Name = name;
        Age = age;                    // goes through the validating setter above
        EnrollmentDate = DateTime.Today; // fine: readonly field set inside a constructor
    }

    // Chained constructor: ": this(...)" forwards to the constructor above so
    // the validation/defaulting logic lives in exactly one place.
    public Student(string name) : this(name, 18)
    {
        // body can stay empty — everything needed already happened in the
        // constructor we chained to.
    }

    // ---- Overloaded CalculateGrade ----
    // Overload 1: grade a single score.
    public char CalculateGrade(int score) => score switch
    {
        >= 90 => 'A',
        >= 75 => 'B',
        >= 60 => 'C',
        _ => 'F'
    };

    // Overload 2: grade based on the average of several scores.
    // Same method name, different parameter type/count — this is the
    // "overload" (compile-time selection based on the argument list).
    public char CalculateGrade(int[] scores)
    {
        double average = scores.Length == 0 ? 0 : (double)scores.Sum() / scores.Length;
        return CalculateGrade((int)average); // reuse the single-score overload
    }
}
