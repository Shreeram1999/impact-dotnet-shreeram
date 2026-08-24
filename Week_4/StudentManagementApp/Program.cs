using Microsoft.Extensions.DependencyInjection;
using StudentManagementApp;
using StudentManagementApp.Controllers;
using StudentManagementApp.Data;
using StudentManagementApp.Models;
using StudentManagementApp.Services;
using StudentManagementApp.Views;

// Task 4.7 first, so the "manual DI" step this refactor grew out of is
// still something you can actually see run, not just read about.
ManualDiBootstrap.Demonstrate();

// Task 4.8 - the same object graph ManualDiBootstrap built by hand above,
// built instead by registering each type with the Microsoft DI container
// and letting it resolve the whole dependency chain for us.
Console.WriteLine("--- Task 4.8: Microsoft DI container wiring ---");

var services = new ServiceCollection();

// Every registration here is Singleton. In ASP.NET Core, Scoped exists to
// give each HTTP request its own instance - but a console app has no
// requests, so there is no natural "scope" boundary for Scoped to attach
// to (see DI_Notes.md). Transient is actively wrong here: if IRepository<T>
// were Transient, every constructor that asked for one would get a BRAND
// NEW, empty List<T> - GetAll() right after Add() could come back empty,
// because the two calls would silently be talking to two different
// repositories. Singleton is the only lifetime where "the app has one
// student list for its whole run" - which is what a console CRUD app
// actually needs - holds true.
services.AddSingleton<TransactionLog>();
services.AddSingleton<IRepository<Student>, InMemoryRepository<Student>>();
services.AddSingleton<IRepository<Teacher>, InMemoryRepository<Teacher>>();
services.AddSingleton<IStudentService, StudentService>();
services.AddSingleton<ITeacherService, TeacherService>();
services.AddSingleton<StudentView>();
services.AddSingleton<TeacherView>();
services.AddSingleton<ConsoleView>();
services.AddSingleton<StudentController>();
services.AddSingleton<TeacherController>();
services.AddSingleton<AppController>();

using var provider = services.BuildServiceProvider();

// The container works out, on its own, that AppController needs a
// StudentController and a TeacherController, each of which needs an
// IStudentService/ITeacherService and a View, each of which needs an
// IRepository<T> and the shared TransactionLog - none of that wiring is
// written out by hand anywhere below, unlike ManualDiBootstrap above.
var appController = provider.GetRequiredService<AppController>();
appController.Run();
