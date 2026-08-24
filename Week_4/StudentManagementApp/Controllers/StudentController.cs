using StudentManagementApp.Services;
using StudentManagementApp.Views;

namespace StudentManagementApp.Controllers;

// Task 4.6 - Controller = orchestration only.
//
// Read the loop below end to end: every branch is "read a menu choice ->
// call the service -> hand whatever the service returned straight to the
// view". There is no string formatting here (that's PrintStudents'/
// ShowMessage's job) and no direct access to any List<T> - GetAll() comes
// back from IStudentService, never from a repository this class doesn't
// even have a reference to.
public class StudentController
{
    private readonly IStudentService service;
    private readonly StudentView view;

    public StudentController(IStudentService service, StudentView view)
    {
        this.service = service;
        this.view = view;
    }

    public void Run()
    {
        while (true)
        {
            var choice = view.ShowMenu();
            switch (choice)
            {
                case "1":
                    var newStudent = view.PromptForStudent();
                    var addResult = service.AddStudent(newStudent.Name, newStudent.Age, newStudent.RollNumber, newStudent.Email);
                    view.ShowMessage(addResult.Message);
                    break;

                case "2":
                    view.PrintStudents(service.GetAll());
                    break;

                case "3":
                    var foundId = view.PromptForId();
                    var found = service.GetById(foundId);
                    if (found is null)
                        view.ShowMessage($"No student found with Id {foundId}.");
                    else
                        view.PrintStudents(new[] { found });
                    break;

                case "4":
                    var updateId = view.PromptForId();
                    var updatedFields = view.PromptForStudent();
                    var updateResult = service.UpdateStudent(updateId, updatedFields.Name, updatedFields.Age, updatedFields.RollNumber, updatedFields.Email);
                    view.ShowMessage(updateResult.Message);
                    break;

                case "5":
                    var deleteId = view.PromptForId();
                    var deleteResult = service.DeleteStudent(deleteId);
                    view.ShowMessage(deleteResult.Message);
                    break;

                case "0":
                    return;

                default:
                    view.ShowMessage("Invalid choice.");
                    break;
            }
        }
    }
}
