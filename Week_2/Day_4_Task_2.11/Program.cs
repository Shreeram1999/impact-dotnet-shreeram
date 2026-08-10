// `new Repository<Student>()` "fills in" T with Student for this instance.
// From here on, `students` only ever accepts/returns Student objects - if
// you tried `students.Add(new Product(...))` it simply wouldn't compile.
var students = new Repository<Student>();
students.Add(new Student { Name = "Asha", Grade = "A" });
students.Add(new Student { Name = "Rohit", Grade = "B" });

// A completely separate Repository<Product> instance, using the exact
// same Repository<T> class definition as above, just with T = Product
// this time.
var products = new Repository<Product>();
products.Add(new Product { Name = "Keyboard", Price = 1299m });
products.Add(new Product { Name = "Mouse", Price = 599m });

Console.WriteLine("Students:");
foreach (var s in students.GetAll())
    Console.WriteLine($"  {s}");

Console.WriteLine("Products:");
foreach (var p in products.GetAll())
    Console.WriteLine($"  {p}");

// Update: build a new Student with the FIRST student's Id but Grade "A+" -
// Repository.Update() finds the old one by matching Id and swaps it out.
// Delete: removes the first product by its Id.
var firstStudent = students.GetAll()[0];
students.Update(new Student { Id = firstStudent.Id, Name = firstStudent.Name, Grade = "A+" });
products.Delete(products.GetAll()[0].Id);

Console.WriteLine();
Console.WriteLine("After Update(student grade) and Delete(first product):");
foreach (var s in students.GetAll())
    Console.WriteLine($"  {s}");
foreach (var p in products.GetAll())
    Console.WriteLine($"  {p}");
