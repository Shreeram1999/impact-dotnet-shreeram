// A `record` is used here (instead of a full `class`) because Employee is
// just a bundle of data we'll query with LINQ in Program.cs - records give
// us a constructor, ToString(), and equality for free, without writing any
// of that boilerplate ourselves.
public record Employee(string Name, string Department, double Salary, DateTime JoiningDate);
