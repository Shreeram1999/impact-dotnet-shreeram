using StudentManagementApp.Services;
using StudentManagementApp.Views;

namespace StudentManagementApp.Controllers;

// Task 4.10 (stretch) - Teacher's controller, same orchestration-only shape
// as StudentController.
public class TeacherController
{
    private readonly ITeacherService service;
    private readonly TeacherView view;

    public TeacherController(ITeacherService service, TeacherView view)
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
                    var newTeacher = view.PromptForTeacher();
                    var addResult = service.AddTeacher(newTeacher.Name, newTeacher.Email, newTeacher.Designation);
                    view.ShowMessage(addResult.Message);
                    break;

                case "2":
                    view.PrintTeachers(service.GetAll());
                    break;

                case "3":
                    var foundId = view.PromptForId();
                    var found = service.GetById(foundId);
                    if (found is null)
                        view.ShowMessage($"No teacher found with Id {foundId}.");
                    else
                        view.PrintTeachers(new[] { found });
                    break;

                case "4":
                    var updateId = view.PromptForId();
                    var updatedFields = view.PromptForTeacher();
                    var updateResult = service.UpdateTeacher(updateId, updatedFields.Name, updatedFields.Email, updatedFields.Designation);
                    view.ShowMessage(updateResult.Message);
                    break;

                case "5":
                    var deleteId = view.PromptForId();
                    var deleteResult = service.DeleteTeacher(deleteId);
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
