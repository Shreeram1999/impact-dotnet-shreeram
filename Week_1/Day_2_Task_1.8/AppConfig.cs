// #region/#endregion carry no meaning to the compiler at all — they exist
// purely so an editor (Visual Studio / VS Code) can show a collapsible
// outline. They're a readability tool for grouping related members.
public class AppConfig
{
    #region Fields
    private readonly int _trialDaysRemaining;
    #endregion

    #region Properties
    public string AppName { get; set; }
    #endregion

    #region Constructors
    public AppConfig(string appName, int trialDaysRemaining)
    {
        AppName = appName;
        _trialDaysRemaining = trialDaysRemaining;
    }
    #endregion

    #region Methods
    public void PrintSummary()
    {
        Console.WriteLine($"{AppName}: {_trialDaysRemaining} trial day(s) remaining.");
    }
    #endregion
}
