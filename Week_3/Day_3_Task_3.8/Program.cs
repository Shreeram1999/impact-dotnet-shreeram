// One IUnitOfWork gives us access to BOTH repositories through a single
// object, instead of constructing StudentRepository and CourseRepository
// separately and having to pass them around independently.
IUnitOfWork unitOfWork = new UnitOfWork();

unitOfWork.Students.Add(new Student { Name = "Asha" });
unitOfWork.Students.Add(new Student { Name = "Rohit" });
unitOfWork.Courses.Add(new Course { Title = "Intro to C#" });

Console.WriteLine("Students:");
foreach (var student in unitOfWork.Students.GetAll())
    Console.WriteLine($"  {student}");

Console.WriteLine("Courses:");
foreach (var course in unitOfWork.Courses.GetAll())
    Console.WriteLine($"  {course}");

// Save() is the single point where "everything we changed above" gets
// committed - in a real database-backed Unit of Work, this is the one
// place a transaction would actually be committed.
var savedCount = unitOfWork.Save();
Console.WriteLine();
Console.WriteLine($"Save() reports {savedCount} total records across both repositories.");
