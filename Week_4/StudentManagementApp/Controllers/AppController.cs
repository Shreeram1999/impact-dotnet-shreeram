using StudentManagementApp.Services;
using StudentManagementApp.Views;

namespace StudentManagementApp.Controllers;

// Task 4.10 (stretch) - "a menu to switch between students and teachers".
// This top-level controller does exactly the same job StudentController and
// TeacherController each do at their own level - read a choice, delegate -
// just one level higher up, choosing which entity's controller gets to run.
public class AppController
{
    private readonly StudentController studentController;
    private readonly TeacherController teacherController;
    private readonly ConsoleView view;
    private readonly TransactionLog transactionLog;

    public AppController(
        StudentController studentController,
        TeacherController teacherController,
        ConsoleView view,
        TransactionLog transactionLog)
    {
        this.studentController = studentController;
        this.teacherController = teacherController;
        this.view = view;
        this.transactionLog = transactionLog;
    }

    public void Run()
    {
        while (true)
        {
            var choice = view.ShowMainMenu();
            switch (choice)
            {
                case "1":
                    studentController.Run();
                    break;

                case "2":
                    teacherController.Run();
                    break;

                case "3":
                    view.PrintTransactionLog(transactionLog.History);
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
