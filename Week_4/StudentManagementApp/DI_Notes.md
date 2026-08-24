# Day 4 notes: manual DI -> Microsoft DI container

## Task 4.7 - manual DI

`ManualDiBootstrap.Demonstrate()` (run first, from `Program.cs`) builds the
exact same object graph the rest of the app uses, entirely by hand:

```csharp
IRepository<Student> repository = new InMemoryRepository<Student>();
var transactionLog = new TransactionLog();
IStudentService service = new StudentService(repository, transactionLog);
var view = new StudentView();
var controller = new StudentController(service, view);
```

Every dependency below `IRepository<Student>` is written against an
interface or a plain concrete class - `StudentService` only knows about
`IRepository<Student>`, never about `InMemoryRepository<Student>` by name.
That's what makes the **first line** the only line that would need to
change to swap in a different repository implementation - `StudentService`,
`StudentController`, and `StudentView` would all stay exactly as they are.

## Task 4.8 - refactoring to the Microsoft DI container

`Program.cs` builds the same graph again, but by registering types instead
of `new`-ing them up, then asking the container to resolve the entry point:

```csharp
services.AddSingleton<IRepository<Student>, InMemoryRepository<Student>>();
services.AddSingleton<IStudentService, StudentService>();
services.AddSingleton<StudentView>();
services.AddSingleton<StudentController>();
...
var appController = provider.GetRequiredService<AppController>();
```

The container works out the constructor chain on its own - nobody writes
`new StudentService(repository, transactionLog)` anywhere in `Program.cs`.
Behaviorally the app is identical: same menu, same CRUD operations, same
shared transaction log.

## Why every registration is Singleton

ASP.NET Core adds a third lifetime, Scoped, that ties an instance to one
HTTP request. A console app has no requests and no request boundary for
Scoped to attach to, so Scoped and Singleton would behave identically here -
Singleton is the honest name for what's actually happening.

Transient is the one that would silently break the app: if
`IRepository<Student>` were registered `AddTransient`, every class that asks
for one would get a **new, empty `List<T>`**. `StudentController` would add
a student to one repository instance and `GetAll()` would read from a
different, empty one - the app would look broken with no exception thrown
anywhere. Singleton is the only lifetime where "one student list, and one
transaction log, for the whole run of the app" - which is what a console
CRUD app actually needs - holds true.
