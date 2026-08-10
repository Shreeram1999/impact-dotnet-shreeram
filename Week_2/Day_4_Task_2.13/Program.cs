// Task 2.13 - LINQ (Language Integrated Query), written two ways.
// LINQ lets you filter/sort/group/transform collections declaratively -
// you describe WHAT you want, not the step-by-step loop for HOW to get it.
// There are two equivalent styles in C#:
//   - QUERY syntax: `from x in list where ... orderby ... select ...`,
//     which reads a bit like a SQL query.
//   - METHOD syntax: `list.Where(...).OrderBy(...)`, chaining extension
//     methods together instead.
// Both compile down to the exact same method calls under the hood - query
// syntax is really just "syntax sugar" for method syntax. Below, every
// query is written both ways and then compared with SequenceEqual to prove
// they really do return identical results.
List<Employee> employees =
[
    new("Asha", "Engineering", 85000, new DateTime(2019, 3, 1)),
    new("Rohit", "Engineering", 62000, new DateTime(2021, 7, 15)),
    new("Meera", "Engineering", 45000, new DateTime(2023, 1, 10)),
    new("Karan", "Sales", 55000, new DateTime(2020, 5, 20)),
    new("Divya", "Sales", 48000, new DateTime(2022, 11, 3)),
    new("Farhan", "Sales", 72000, new DateTime(2018, 9, 12)),
    new("Priya", "HR", 51000, new DateTime(2021, 2, 28)),
    new("Vikram", "HR", 39000, new DateTime(2023, 6, 1)),
    new("Neha", "Marketing", 67000, new DateTime(2019, 10, 5)),
    new("Suresh", "Marketing", 58000, new DateTime(2020, 12, 19)),
    new("Anjali", "Marketing", 44000, new DateTime(2022, 4, 8)),
];

// --- Query 1: filter salary > 50000, order by salary desc ---
// `where` keeps only matching items; `orderby ... descending` sorts them,
// highest salary first. `.Where(...)` + `.OrderByDescending(...)` do the
// exact same two steps in method syntax.

var highEarnersQuery =
    from e in employees
    where e.Salary > 50000
    orderby e.Salary descending
    select e;

var highEarnersMethod = employees
    .Where(e => e.Salary > 50000)
    .OrderByDescending(e => e.Salary);

Console.WriteLine("High earners (salary > 50000), highest first:");
foreach (var e in highEarnersQuery)
    Console.WriteLine($"  {e.Name} - {e.Salary:C}");
Console.WriteLine($"Query syntax and method syntax match: {highEarnersQuery.SequenceEqual(highEarnersMethod)}");

// --- Query 2: group by department, with a count and average salary ---
// `group e by e.Department into g` buckets employees by department into
// groups named `g`; each `g` is itself a mini-collection of employees plus
// a `.Key` (the department name). We then reduce each group down to a
// small summary object using an ANONYMOUS TYPE (`new { ... }` with no type
// name) - handy for throwaway shapes you only need right here.
// `.GroupBy(...)` in method syntax does the same bucketing.

var byDepartmentQuery =
    from e in employees
    group e by e.Department into g
    select new { Department = g.Key, Count = g.Count(), AverageSalary = g.Average(x => x.Salary) };

var byDepartmentMethod = employees
    .GroupBy(e => e.Department)
    .Select(g => new { Department = g.Key, Count = g.Count(), AverageSalary = g.Average(x => x.Salary) });

Console.WriteLine();
Console.WriteLine("Department summary:");
foreach (var d in byDepartmentQuery)
    Console.WriteLine($"  {d.Department}: {d.Count} employees, avg {d.AverageSalary:C}");
Console.WriteLine($"Query syntax and method syntax match: {byDepartmentQuery.SequenceEqual(byDepartmentMethod)}");

// --- Query 3: project each employee down to just { Name, Experience } ---
// "Projecting" means reshaping each item into something new - here we
// throw away Department/Salary/JoiningDate and keep only a computed
// Experience (years since joining) alongside Name. Again, both an
// anonymous type version (query syntax) and a `.Select(...)` version
// (method syntax) produce the same reshaped data.

var today = DateTime.Today;

var experienceQuery =
    from e in employees
    select new { e.Name, Experience = today.Year - e.JoiningDate.Year };

var experienceMethod = employees
    .Select(e => new { e.Name, Experience = today.Year - e.JoiningDate.Year });

Console.WriteLine();
Console.WriteLine("Name and years of experience:");
foreach (var x in experienceQuery)
    Console.WriteLine($"  {x.Name}: {x.Experience} yrs");
Console.WriteLine($"Query syntax and method syntax match: {experienceQuery.SequenceEqual(experienceMethod)}");
