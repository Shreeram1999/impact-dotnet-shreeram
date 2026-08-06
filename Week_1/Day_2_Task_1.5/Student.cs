namespace SchoolManagement
{
    // A namespace is just a named container for types, used to avoid name
    // collisions when different parts of a codebase (or different libraries)
    // happen to declare a class with the same name.
    public class Student
    {
        public string Name { get; set; } = string.Empty;

        public void Display()
        {
            Console.WriteLine($"Student Name: {Name}");
        }
    }
}
