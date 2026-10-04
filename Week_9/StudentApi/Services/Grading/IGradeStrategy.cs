namespace StudentApi.Services.Grading;

// Task 5.8 - the applied Strategy pattern, same shape as Week 3's
// IPaymentStrategy (Day_3_Task_3.7): pull "how do I describe this score?"
// out into its own interchangeable object behind one interface, so
// StudentsController never needs an if/switch on which display format the
// caller asked for.
public interface IGradeStrategy
{
    string Describe(int score);
}
